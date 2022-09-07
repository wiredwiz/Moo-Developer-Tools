using Krypton.Toolkit;
using Org.Edgerunner.Moo.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Org.Edgerunner.Moo.Editor
{
   public partial class WorldManager : KryptonForm
   {
      public WorldManager()
      {
         InitializeComponent();
      }

      /// <summary>
      /// Occurs when the connect button is clicked.
      /// </summary>
      public event EventHandler<WorldConfiguration> ConnectToWorld;

      /// <summary>
      /// Gets or sets the address book.
      /// </summary>
      /// <value>The address book.</value>
      protected virtual AddressBook AddressBook { get; set; }

      /// <summary>
      /// Gets or sets the source file path.
      /// </summary>
      /// <value>The source file.</value>
      public virtual string SourceFile { get; set; }

      /// <summary>
      /// Loads an <see cref="AddressBook"/> from file.
      /// </summary>
      /// <param name="filePath">The file path.</param>
      public virtual void LoadFromFile(string filePath)
      {
         SourceFile = filePath;
         var addressBook = AddressBook.LoadFromFile(SourceFile);
         LoadAddressBook(addressBook);
      }

      /// <summary>
      /// Saves the internal <see cref="AddressBook"/> to file.
      /// </summary>
      /// <param name="filePath">The file path.</param>
      public virtual void SaveToFile(string filePath)
      {
         AddressBook.SaveToFile(filePath);
      }

      /// <summary>
      /// Loads the address book.
      /// </summary>
      /// <param name="addressBook">The address book.</param>
      public virtual void LoadAddressBook(AddressBook addressBook)
      {
         AddressBook = addressBook;
         lstWorlds.DataSource = AddressBook.Worlds;
      }

      protected virtual void UpdateSourceData(AddressBook book)
      {
         lstWorlds.DataSource = null;
         lstWorlds.SuspendLayout();
         lstWorlds.Items.Clear();
         lstWorlds.DataSource = book.Worlds;
         lstWorlds.DisplayMember = "Name";
         lstWorlds.ValueMember = "Name";
         lstWorlds.ResumeLayout();
      }

      /// <summary>
      /// Gets the selected world.
      /// </summary>
      /// <returns>The selected <see cref="WorldConfiguration"/> instance.</returns>
      protected virtual WorldConfiguration GetSelectedWorld()
      {
         if (lstWorlds.SelectedIndex == -1)
            return null;

         return lstWorlds.SelectedItem as WorldConfiguration;
      }

      private void btnMoveDown_Click(object sender, EventArgs e)
      {
         var selectedIndex = lstWorlds.SelectedIndex;
         if (selectedIndex == -1 || selectedIndex == AddressBook.Worlds.Count - 1)
            return;

         lstWorlds.SuspendLayout();
         var selectedWorld = GetSelectedWorld();
         var existing = AddressBook.Worlds[selectedIndex + 1];
         AddressBook.Worlds[selectedIndex + 1] = selectedWorld;
         AddressBook.Worlds[selectedIndex] = existing;
         SaveToFile(SourceFile);
         UpdateSourceData(AddressBook);
         lstWorlds.SelectedIndex = selectedIndex + 1;
         lstWorlds.ResumeLayout();
      }

      private void btnMoveUp_Click(object sender, EventArgs e)
      {
         var selectedIndex = lstWorlds.SelectedIndex;
         if (selectedIndex is -1 or 0)
            return;

         lstWorlds.SuspendLayout();
         var selectedWorld = GetSelectedWorld();
         var existing = AddressBook.Worlds[selectedIndex - 1];
         AddressBook.Worlds[selectedIndex - 1] = selectedWorld;
         AddressBook.Worlds[selectedIndex] = existing;
         SaveToFile(SourceFile);
         UpdateSourceData(AddressBook);
         lstWorlds.SelectedIndex = selectedIndex - 1;
         lstWorlds.ResumeLayout();
      }

      private void btnDelete_Click(object sender, EventArgs e)
      {
         var selectedIndex = lstWorlds.SelectedIndex;
         if (selectedIndex == -1)
            return;

         lstWorlds.SuspendLayout();
         AddressBook.Worlds.RemoveAt(selectedIndex);
         SaveToFile(SourceFile);
         if (selectedIndex == AddressBook.Worlds.Count)
            lstWorlds.SelectedIndex = selectedIndex - 1;
         UpdateSourceData(AddressBook);
         //selectedIndex = Math.Min(selectedIndex, AddressBook.Worlds.Count - 2);
         //lstWorlds.SelectedIndex = selectedIndex;
         lstWorlds.ResumeLayout();
      }

      private void btnEdit_Click(object sender, EventArgs e)
      {
         var selectedIndex = lstWorlds.SelectedIndex;
         if (selectedIndex == -1)
            return;

         var world = GetSelectedWorld();
         var configurator = new WorldConfigurator();
         configurator.World = world;
         var result = configurator.ShowDialog(this);
         if (result != DialogResult.OK)
            return;

         lstWorlds.SuspendLayout();
         AddressBook.Worlds[selectedIndex] = configurator.World;
         UpdateSourceData(AddressBook);
         lstWorlds.SelectedIndex = selectedIndex;
         SaveToFile(SourceFile);
         lstWorlds.ResumeLayout();
      }

      private void btnConnect_Click(object sender, EventArgs e)
      {
         var selectedIndex = lstWorlds.SelectedIndex;
         if (selectedIndex == -1)
            return;

         ConnectToWorld?.Invoke(this, GetSelectedWorld());
         Close();
      }

      private void btnNew_Click(object sender, EventArgs e)
      {
         var selectedIndex = lstWorlds.SelectedIndex;
         var config = new WorldConfiguration("New", string.Empty, 0);
         AddressBook.Worlds.Add(config);
         SaveToFile(SourceFile);
         UpdateSourceData(AddressBook);
         if (selectedIndex != -1)
            lstWorlds.SelectedIndex = selectedIndex;
         lstWorlds.ResumeLayout();
      }
   }
}
