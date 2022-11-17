using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Logging.Debug;
using TextSearch;
using TextSearch.Indexes;
using TextSearch.Lexers;
using Timer = System.Windows.Forms.Timer;

namespace TextSearchApp
{
	public partial class TextIndexAndLookupWindow : Form
	{
		private readonly TextSearchCatalog<string> textSearchCatalog;
		private readonly Timer searchStartTimer;

		public TextIndexAndLookupWindow()
		{
			var debugLoggerProvider = new DebugLoggerProvider();
			var debugLogger = debugLoggerProvider.CreateLogger(nameof(TextSearchCatalog<string>));

			this.textSearchCatalog = new TextSearchCatalog<string>
			(
				new StringLexerFactory(Encoding.UTF8),
				new ConcurrentDictionaryIndexBuilderFactory(),
				debugLogger
			);
			this.searchStartTimer = new Timer();
			this.searchStartTimer.Interval = 500; // 500ms
			this.searchStartTimer.Tick += this.PerformSearch;

			this.InitializeComponent();

			this.SubscribeForDebugLog();
			this.SubscribeForCatalogUpdates();
		}

		private void pathList_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.removePathButton.Enabled = this.pathList.SelectedIndices.Count > 0;
		}

		private void addPathButton_Click(object sender, EventArgs e)
		{
			var selectedPaths = Array.Empty<string>();
			if (sender == this.addFileButton)
			{
				var openFileDialog = new OpenFileDialog();
				openFileDialog.CheckFileExists = true;
				openFileDialog.ValidateNames = true;
				openFileDialog.Multiselect = true;
				var result = openFileDialog.ShowDialog();
				if (result != DialogResult.OK || openFileDialog.FileNames.Length == 0)
				{
					return;
				}
				selectedPaths = openFileDialog.FileNames;
			}
			else if (sender == this.addDirectoryButton)
			{
				var openDirectoryDialog = new FolderBrowserDialog();
				openDirectoryDialog.ShowNewFolderButton = false;
				var result = openDirectoryDialog.ShowDialog();
				if (result != DialogResult.OK || string.IsNullOrEmpty(openDirectoryDialog.SelectedPath))
				{
					return;
				}
				selectedPaths = new[] { openDirectoryDialog.SelectedPath };
			}

			foreach (var path in selectedPaths)
			{
				if (HasPathInList(path))
				{
					continue;
				}

				if (Directory.Exists(path))
				{
					var directoryInfo = new DirectoryInfo(path);
					var indexingTask = this.textSearchCatalog.IndexDirectoryAsync(directoryInfo, trackChanges: true);
					var searchSource = new SearchSource(indexingTask, directoryInfo.FullName);
					this.pathList.Items.Add(searchSource);

					UpdatePathListOnCompletion(indexingTask);
				}
				else if (File.Exists(path))
				{
					var fileInfo = new FileInfo(path);
					var indexingTask = this.textSearchCatalog.IndexFileAsync(fileInfo, trackChanges: true);
					var searchSource = new SearchSource(indexingTask, fileInfo.FullName);
					this.pathList.Items.Add(searchSource);

					UpdatePathListOnCompletion(indexingTask);
				}
			}

			this.RequestSearchResultUpdate();

			bool HasPathInList(string path)
			{
				foreach (var item in this.pathList.Items)
				{
					if (item is SearchSource searchSource && string.Equals(searchSource.Path, path, StringComparison.Ordinal))
					{
						return true;
					}
				}

				return false;
			}
		}

		private void removePathButton_Click(object sender, EventArgs e)
		{
			while (this.pathList.SelectedItems.Count > 0)
			{
				var selectedItem = this.pathList.SelectedItems[0];
				if (selectedItem is SearchSource searchSource)
				{
					searchSource.Dispose();
				}
				this.pathList.Items.Remove(selectedItem);
			}

			this.RequestSearchResultUpdate();
		}

		private void searchBox_TextChanged(object sender, EventArgs e)
		{
			this.RequestSearchResultUpdate();
		}

		private void PerformSearch(object sender, EventArgs e)
		{
			this.searchStartTimer.Enabled = false;
			this.searchResultBox.Items.Clear();
			if (string.IsNullOrEmpty(this.searchBox.Text))
			{
				return;
			}

			var word = this.searchBox.Text.Trim();
			foreach (var foundFile in this.textSearchCatalog.Search(word))
			{
				this.searchResultBox.Items.Add(foundFile.FullName);
			}

			if (this.searchResultBox.Items.Count == 0)
			{
				this.searchResultBox.Items.Add($"Nothing found for \"{word}\".");
			}
		}

		private void RequestSearchResultUpdate()
		{
			this.searchStartTimer.Stop();
			this.searchStartTimer.Interval = 500; // 500ms
			this.searchStartTimer.Start();
		}
		private async void UpdatePathListOnCompletion(Task<IDisposable> indexingTask)
		{
			try
			{
				await indexingTask.ConfigureAwait(true); // capture UI synchronization context
			}
			catch { /* ignore indexing errors */ }

			for (var i = 0; i < this.pathList.Items.Count; i++)
			{
				this.pathList.Items[i] = this.pathList.Items[i]; // force re-render text
			}

		}
		private void SubscribeForCatalogUpdates()
		{
			this.textSearchCatalog.Events
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(_ => this.RequestSearchResultUpdate());
		}
		private void SubscribeForDebugLog()
		{
			Debug.Listeners.Add(new TextBoxTraceListener(this.logsTextBox));
		}
	}
}
