using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using VortexLauncherNet.MVVM;

namespace VortexLauncherNet.Views
{
	/// <summary> Interaction logic for GroupBoxEx.xaml </summary>
	[ContentProperty("MainContent")]
	public partial class GroupBoxEx : UserControl
	{
		/*static GroupBoxEx()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(GroupBoxEx),
					   new FrameworkPropertyMetadata(typeof(GroupBoxEx)));
		}*/

		public GroupBoxEx()
		{
			ContentVisibility = Visibility.Visible;
			ToggleVisibility = new RelayCommand(_ =>
				ContentVisibility = ContentVisibility == Visibility.Collapsed
					? Visibility.Visible
					: Visibility.Collapsed
			);

			InitializeComponent();
		}

		public ICommand ToggleVisibility { get; } 

		// ------------------------------

		public Visibility ContentVisibility
		{
			get => (Visibility)GetValue(ContentVisibilityProperty);
			set => SetValue(ContentVisibilityProperty, value);
		}
		public static readonly DependencyProperty ContentVisibilityProperty = DependencyProperty.Register("ContentVisibility", typeof(Visibility), typeof(GroupBoxEx), null);

		// ------------------------------

		public string HeaderText
		{
			get => (string)GetValue(HeaderTextProperty);
			set => SetValue(HeaderTextProperty, value);
		}
		public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register("HeaderText", typeof(string), typeof(GroupBoxEx), null);

		// ------------------------------

		public object MainContent
		{
			get => GetValue(MainContentProperty);
			set => SetValue(MainContentProperty, value);
		}
		public static readonly DependencyProperty MainContentProperty = DependencyProperty.Register("MainContent", typeof(object), typeof(GroupBoxEx), null);
	}
}
