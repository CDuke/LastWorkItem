using System.Windows;
using System.Windows.Controls;

namespace CDuke.LastWorkItem
{
	/// <summary>
	/// Interaction logic for LastWorkItemView.xaml
	/// </summary>
	public partial class LastWorkItemView : UserControl
	{
		public LastWorkItemView()
		{
			InitializeComponent();
		}

		/// <summary> 
		/// Parent section. 
		/// </summary> 
		public LastWorkItemSection ParentSection
		{
			get { return (LastWorkItemSection)GetValue(ParentSectionProperty); }
			set { SetValue(ParentSectionProperty, value); }
		}
		public static readonly DependencyProperty ParentSectionProperty =
			DependencyProperty.Register("ParentSection", typeof(LastWorkItemSection), typeof(LastWorkItemSection));

		/// <summary> 
		/// Set last work item event handler. 
		/// </summary> 
		private void SetLastWorkItemLink_Click(object sender, RoutedEventArgs e)
		{
			this.ParentSection.SetLastWorkItem();
		}
	}
}
