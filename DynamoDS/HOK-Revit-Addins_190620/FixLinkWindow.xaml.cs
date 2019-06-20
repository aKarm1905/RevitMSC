using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using HOK.ModelManager.GoogleDocs;
using System.Collections.ObjectModel;

namespace HOK.ModelManager.ReplicateViews
{
    /// <summary>
    /// Interaction logic for FixLinkWindow.xaml
    /// </summary>
    public partial class FixLinkWindow : Window
    {
        private ViewMapClass fixedViewMap = null;

        private ModelInfo sourceInfo = null;
        private ModelInfo recipientInfo = null;

        private Dictionary<int, ViewProperties> sourceViews = new Dictionary<int, ViewProperties>();
        private Dictionary<int, ViewProperties> recipientViews = new Dictionary<int, ViewProperties>();
        private List<LinkInfo> listLinkInfo = new List<LinkInfo>();

        public ViewMapClass FixedViewMap { get { return fixedViewMap; } set { fixedViewMap = value; } }

        public FixLinkWindow(ViewMapClass viewMapClass)
        {
            fixedViewMap = viewMapClass;
            sourceInfo = fixedViewMap.SourceInfo;
            recipientInfo = fixedViewMap.RecipientInfo;

            sourceViews = viewMapClass.SourceViews;
            recipientViews = viewMapClass.RecipientViews;
            listLinkInfo = viewMapClass.LinkInfoList; //from recepient model only related to the source model

            InitializeComponent();

            textSourceTitle.Text = sourceInfo.DocTitle;
            textRecipientTitle.Text = recipientInfo.DocTitle;
           
            DisplayViews();
        }

        private void DisplayViews()
        {
            try
            {
                SortDescription sd = new SortDescription("Content", ListSortDirection.Ascending);

                listViewSource.Items.Clear();
                foreach (ViewProperties vp in sourceViews.Values)
                {
                    if (null != vp.LinkedView) { continue; }
                    ListViewItem item = new ListViewItem();
                    item.Content = vp.ViewName;
                    item.Tag = vp;
                    item.ToolTip = "Id: " + vp.ViewId;
                    if (vp.Status == LinkStatus.MissingFromRecipient)
                    {
                        item.Foreground = Brushes.Red;
                        item.ToolTip = "Missing From Recipient. Id: "+vp.ViewId;
                    }
                    listViewSource.Items.Add(item);
                }
                listViewSource.Items.SortDescriptions.Add(sd);
          

                listViewRecipient.Items.Clear();
                foreach (ViewProperties vp in recipientViews.Values)
                {
                    if (null != vp.LinkedView) { continue; }
                    ListViewItem item = new ListViewItem();
                    item.Content = vp.ViewName;
                    item.Tag = vp;
                    item.ToolTip = "Id: " + vp.ViewId;
                    if (vp.Status == LinkStatus.MissingFromSource)
                    {
                        item.Foreground = Brushes.Red;
                        item.ToolTip = "Missing From Source. Id: "+vp.ViewId;
                    }
                    listViewRecipient.Items.Add(item);
                }
                listViewRecipient.Items.SortDescriptions.Add(sd);

                listViewMap.ItemsSource = null;
                List<LinkInfo> linkedItems = new List<LinkInfo>();
                foreach (LinkInfo info in listLinkInfo)
                {
                    if (info.ItemStatus == LinkItemStatus.Deleted) { continue; }
                    int sourceId = info.SourceItemId;
                    int destId = info.DestItemId;
                    if (sourceViews.ContainsKey(sourceId) && recipientViews.ContainsKey(destId))
                    {
                        linkedItems.Add(info);
                    }
                }
                listViewMap.ItemsSource = linkedItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display the list of views.\n"+ex.Message, "Display Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != listViewSource.SelectedItem && null != listViewRecipient.SelectedItem)
                {
                    if (listViewSource.SelectedItems.Count > 1)
                    {
                        MessageBox.Show("Please select only one item from the list of the source view.", "Multiple Source View Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    ListViewItem sItem = (ListViewItem)listViewSource.SelectedItem;
                    ViewProperties sView = (ViewProperties)sItem.Tag;

                    ListViewItem rItem = (ListViewItem)listViewRecipient.SelectedItem;
                    ViewProperties rview = (ViewProperties)rItem.Tag;

                    sView.LinkedView = rview;
                    rview.LinkedView = sView;
                    
                    if(sourceViews.ContainsKey(sView.ViewId))
                    {
                        sourceViews.Remove(sView.ViewId);
                        sourceViews.Add(sView.ViewId, sView);
                    }
                    if (recipientViews.ContainsKey(rview.ViewId))
                    {
                        recipientViews.Remove(rview.ViewId);
                        recipientViews.Add(rview.ViewId, rview);
                    }

                    LinkInfo linkInfo = new LinkInfo();
                    linkInfo.ItemType = LinkItemType.DraftingView;
                    linkInfo.SourceModelName = sourceInfo.DocTitle;
                    linkInfo.SourceModelPath = sourceInfo.DocCentralPath;
                    linkInfo.SourceModelId = sourceInfo.RevitDocumentID;
                    linkInfo.SourceItemId = sView.ViewId;
                    linkInfo.SourceItemName = sView.ViewName;
                    linkInfo.DestModelName = recipientInfo.DocTitle;
                    linkInfo.DestModelPath = recipientInfo.DocCentralPath;
                    linkInfo.DestItemId = rview.ViewId;
                    linkInfo.DestItemName = rview.ViewName;

                    linkInfo.ItemStatus = LinkItemStatus.Added;
                    listLinkInfo.Add(linkInfo);

                    DisplayViews();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add linked items.\n"+ex.Message, "Add View Links", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != listViewMap.SelectedItems)
                {
                    for (int i = listViewMap.SelectedItems.Count - 1; i > -1; i--)
                    {
                        LinkInfo link = (LinkInfo)listViewMap.SelectedItems[i];
                        int index = listLinkInfo.IndexOf(link);
                        listLinkInfo.RemoveAt(index);
                        link.ItemStatus = LinkItemStatus.Deleted;
                        listLinkInfo.Add(link);

                        if (sourceViews.ContainsKey(link.SourceItemId))
                        {
                            ViewProperties vp = sourceViews[link.SourceItemId];
                            vp.LinkedView = null;
                            sourceViews.Remove(vp.ViewId);
                            sourceViews.Add(vp.ViewId, vp);
                        }

                        if (recipientViews.ContainsKey(link.DestItemId))
                        {
                            ViewProperties vp = recipientViews[link.DestItemId];
                            vp.LinkedView = null;
                            recipientViews.Remove(vp.ViewId);
                            recipientViews.Add(vp.ViewId, vp);
                        }
                    }
                    DisplayViews();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete linked items.\n"+ex.Message, "Delete View Links", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                fixedViewMap.SourceViews = sourceViews;
                fixedViewMap.RecipientViews = recipientViews;
                fixedViewMap.LinkInfoList = listLinkInfo;
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply the link settings.\n"+ex.Message, "Apply Links", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonForget_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Remove the record of the links from Google spreadsheet
                if (null != listViewSource.SelectedItems)
                {
                    for (int i = 0; i < listViewSource.SelectedItems.Count; i++)
                    {
                        ListViewItem item = listViewSource.SelectedItems[i] as ListViewItem;
                        ViewProperties vp = (ViewProperties)item.Tag;
                        if (sourceViews.ContainsKey(vp.ViewId))
                        {
                            vp.Status = LinkStatus.None;
                            sourceViews.Remove(vp.ViewId);
                            sourceViews.Add(vp.ViewId, vp);
                        }


                        var linkItems = from litem in listLinkInfo where litem.SourceItemId == vp.ViewId select litem;
                        if (linkItems.Count() > 0)
                        {
                            LinkInfo linkInfo = linkItems.First();
                            int index = listLinkInfo.IndexOf(linkInfo);
                            listLinkInfo.RemoveAt(index);
                            linkInfo.ItemStatus = LinkItemStatus.Deleted;
                            listLinkInfo.Add(linkInfo);
                        }
                    }
                    DisplayViews();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to remove records of the links.\n"+ex.Message, "Remove Links Records", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }
}
