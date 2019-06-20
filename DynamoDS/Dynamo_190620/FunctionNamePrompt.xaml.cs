﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Dynamo.Controls;
using Dynamo.Utilities;
using DynamoUtilities;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Interaction logic for FunctionNamePrompt.xaml
    /// </summary>
    public partial class FunctionNamePrompt : Window
   {
      public FunctionNamePrompt(IEnumerable<string> categories)
      {
         InitializeComponent();

         this.Owner = WpfUtilities.FindUpVisualTree<DynamoView>(this);
         this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

         this.nameBox.Focus();

         var sortedCats = categories.ToList();
         sortedCats.Sort();

         foreach (var item in sortedCats)
         {
            this.categoryBox.Items.Add(item);
         }
      }

      void OK_Click(object sender, RoutedEventArgs e)
      {
          if (string.IsNullOrEmpty(Text))
          {
              MessageBox.Show(Dynamo.Wpf.Properties.Resources.MessageCustomNodeNoName,
                  Dynamo.Wpf.Properties.Resources.CustomNodePropertyErrorMessageBoxTitle,
                  MessageBoxButton.OK, MessageBoxImage.Error);
          }
          else if (PathHelper.IsFileNameInValid(Text))
          {
              MessageBox.Show(Dynamo.Wpf.Properties.Resources.MessageCustomNodeNameInvalid,
                 Dynamo.Wpf.Properties.Resources.CustomNodePropertyErrorMessageBoxTitle,
                 MessageBoxButton.OK, MessageBoxImage.Error);
          }
          else if (string.IsNullOrEmpty(Category))
          {
              MessageBox.Show(Dynamo.Wpf.Properties.Resources.MessageCustomNodeNeedNewCategory,
                  Dynamo.Wpf.Properties.Resources.CustomNodePropertyErrorMessageBoxTitle,
                  MessageBoxButton.OK, MessageBoxImage.Error);
          }
          else
          {
              this.DialogResult = true;
          }
      }

      void Cancel_Click(object sender, RoutedEventArgs e)
      {
         this.DialogResult = false;
      }

      public string Text
      {
         get { return this.nameBox.Text; }
      }

      public string Description
      {
          get { return this.DescriptionInput.Text; }
      }

      public string Category
      {
         get { return this.categoryBox.Text; }
      }
   }
}
