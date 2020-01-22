using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PictureRouletto {
	/// <summary>
	/// MainWindowViewModel
	/// </summary>
	internal class MainWindowViewModel : INotifyPropertyChanged {

		/// <summary>
		/// プロパティが変更されたときにViewに通知するイベント
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#region Property

		/// <summary>
		/// ModelViewが保持してるModel
		/// </summary>
		public MainWindowModel Model { get; } = new MainWindowModel();

		/// <summary>
		/// 表示するビットマップイメージ
		/// </summary>
		public BitmapImage BitmapImage => this.Model.BitmapImage;

		#endregion


		#region Command

		/// <summary>
		/// スタートコマンド
		/// </summary>
		public StartCommand StartCommand => this.Model.StartCommand;

		#endregion

		/// <summary>
		/// View Modelを初期化します。
		/// </summary>
		public MainWindowViewModel() => this.Model.PropertyChanged += (sender, e) => this.PropertyChanged.Invoke(this, e);
	}
}
