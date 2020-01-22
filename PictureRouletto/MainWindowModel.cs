using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PictureRouletto {
	/// <summary>
	/// メインウィンドのモデル
	/// </summary>
	internal class MainWindowModel : INotifyPropertyChanged {

		/// <summary>
		/// プロパティが変更されたときにViewModelに通知するイベント
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#region private field

		/// <summary>
		/// 乱数
		/// </summary>
		private readonly Random random = new Random();

		/// <summary>
		/// 読み込んだ画像配列
		/// </summary>
		private readonly BitmapImage[] bitmapImages;

		/// <summary>
		/// 画像を表示するインデックスのスタック
		/// </summary>
		private readonly Stack<int> indexStack;

		/// <summary>
		/// 画像を表示するインデックスのリスト
		/// </summary>
		private List<int> indexList;

		#endregion

		#region Property

		/// <summary>
		/// 表示するビットマップイメージ
		/// </summary>
		public BitmapImage BitmapImage { get; private set; }

		#endregion

		#region Command

		/// <summary>
		/// スタートコマンド
		/// </summary>
		public StartCommand StartCommand { get; } = new StartCommand();

		#endregion

		/// <summary>
		/// Modelの初期化
		/// </summary>
		public MainWindowModel() {
			string[] imageFilePaths = Directory.GetFiles(@".\images", "*", SearchOption.TopDirectoryOnly);
			IEnumerable<BitmapImage> bitmapImages = from path in imageFilePaths select new BitmapImage(new Uri(Path.GetFullPath(path)));
			this.bitmapImages = bitmapImages.ToArray();
			this.BitmapImage = this.bitmapImages[0];
			this.indexList = new List<int>();
			for (int i = 0; i < this.bitmapImages.Length; i++) {
				this.indexList.Add(i);
			}
			// シャッフル
			this.indexStack = new Stack<int>(this.indexList.OrderBy(x => Guid.NewGuid()));
		}

		/// <summary>
		/// プロパティの変更をViewに通知
		/// </summary>
		/// <param name="propertyName">変更されたプロパティ名</param>
		public void OnPropertyChanded(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


		/// <summary>
		/// ルーレットを非同期に開始します
		/// </summary>
		/// <returns>ルーレットの最後に到達した場合falseが返る</returns>
		public Task<bool> RunRoulettoAsync() => Task<bool>.Run(() => {
			this.indexList = this.indexList.OrderBy(x => Guid.NewGuid()).ToList();
			for (int i = 0; i < 15; i++) {
				this.BitmapImage = this.bitmapImages[this.indexList[i % this.bitmapImages.Length]];
				this.OnPropertyChanded(nameof(this.BitmapImage));
				Thread.Sleep(100);
			}

			this.BitmapImage = this.bitmapImages[this.indexStack.Pop()];
			this.OnPropertyChanded(nameof(this.BitmapImage));

			return this.indexStack.Count == 0;
		});


	}

	internal class StartCommand : ICommand {

		/// <summary>
		/// このコマンドが有効か否か
		/// </summary>
		private bool isActive = true;

		/// <summary>
		/// ルーレットの最後に到達したか
		/// </summary>
		private bool isLast = false;

		/// <summary>
		/// ボタンの有効無効状態の変化を伝えるイベント
		/// </summary>
		public event EventHandler CanExecuteChanged;

		/// <summary>
		/// ボタンの有効無効状態の変化を送信します。
		/// </summary>
		public void OnCanExecuteChanged() => this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);


		/// <summary>
		/// ボタンの有効無効チェック
		/// </summary>
		/// <param name="parameter"></param>
		/// <returns></returns>
		public bool CanExecute(object parameter) => this.isActive;


		/// <summary>
		/// コマンドを実行します。
		/// </summary>
		/// <param name="parameter"></param>
		public async void Execute(object parameter) {
			if (!(parameter is MainWindowModel model)) {
				return;
			}
			if (this.isLast) {
				MessageBox.Show("すべての画像が出されました。アプリケーションを終了します。", "", MessageBoxButton.OK, MessageBoxImage.Information);
				Application.Current.MainWindow.Close();
			}
			this.isActive = false;
			this.OnCanExecuteChanged();
			this.isLast = await model.RunRoulettoAsync();
			this.isActive = true;
			this.OnCanExecuteChanged();
		}
	}
}
