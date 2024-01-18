using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SharpCompress.Readers;

namespace OwlLauncher
{
	public partial class MainWindow : Window
	{
		private readonly HttpClient httpClient;
		private Stopwatch stopwatch;
		public long totalBytes;

		public MainWindow()
		{
			InitializeComponent();
			httpClient = new HttpClient();
		}

		private async void DownloadButton_Click(object sender, RoutedEventArgs e)
		{
			Uri fileUri = new Uri("https://s654sas.storage.yandex.net/rdisk/e09a3c0172ce49682fe393e181a05a57da66bbbaccdf460063314643d9af69f1/65a9316d/g3_u9dEOHjd_Zc1L4wdQcO2z7PM44OZqRy1Dvu89s7tgPkcIeD0ldxbp4CNYxxfeKk9XhIJofoU3lJDuKAuN9w==?uid=0&filename=ULTRAKILLNewDemoPatch1B.zip&disposition=attachment&hash=8dz6mZTeV6%2BPCt4BxUbkKqPUZ1%2BdATRaZ/19UjpjDCKDcQWW8wyaLJSVtKA1LY93q/J6bpmRyOJonT3VoXnDag%3D%3D&limit=0&content_type=application%2Fzip&owner_uid=574548681&fsize=136324427&hid=0b1aa6e540b401867b2967c2da381f7c&media_type=compressed&tknv=v2&rtoken=ZdRgl6ZWNNQP&force_default=no&ycrid=na-4494ff9f72c2b2f716116dc9746b4529-downloader2f&ts=60f38ee6d7540&s=ff6c9237f2547e0532b8a7367c6263f490bd92825b36a0a854793df91027d8bb&pb=U2FsdGVkX1_edTIDcBQAV2BbkYwtNPU12LeAc9Io0OUfnkBtPhATe67SIo4ewJtQmnKavKDUg_dZ5NwFn-pXU8SRJD75nbAv8rBPn__coJo");
			string destinationPath = "D:\\ExtractedFiles\\";

			try
			{
				// Сбросить прогрессбар и связанные данные
				ResetProgressBar();

				// Начать отсчет времени
				stopwatch = Stopwatch.StartNew();

				// Начать загрузку и разархивацию файла
				await DownloadAndExtractFileAsync(fileUri, destinationPath);

				// Остановить отсчет времени
				stopwatch.Stop();

				MessageBox.Show("Файл успешно загружен и разархивирован!");
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка при загрузке и разархивации файла:\n " + ex);
				throw ex;
			}
		}

		private async Task DownloadAndExtractFileAsync(Uri fileUri, string destinationPath)
		{
			using (HttpResponseMessage response = await httpClient.GetAsync(fileUri, HttpCompletionOption.ResponseHeadersRead))
			{
				response.EnsureSuccessStatusCode();

				using (Stream contentStream = await response.Content.ReadAsStreamAsync())
				{
					// Создать директорию, если она не существует
					string extractDirectory = "D:\\ExtractedFiles\\";
					if (!Directory.Exists(extractDirectory))
					{
						Directory.CreateDirectory(extractDirectory);
					}

					totalBytes = response.Content.Headers.ContentLength.GetValueOrDefault();
					// Считано байт
					long totalBytesRead = 0;

					// Продолжить с разархивацией
					using (var reader = ReaderFactory.Open(contentStream))
					{
						while (reader.MoveToNextEntry())
						{
							if (!reader.Entry.IsDirectory)
							{
								
								// Используйте имя файла из архива для создания соответствующего файла в директории
								string filePath = Path.Combine(extractDirectory, reader.Entry.Key);

								// Создать директорию для файла, если она не существует
								Directory.CreateDirectory(Path.GetDirectoryName(filePath));

								using (Stream entryStream = reader.OpenEntryStream())
								using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
								{
									await entryStream.CopyToAsync(fileStream);
									reader.EntryExtractionProgress += Reader_EntryExtractionProgress;
								}
							}
						}
					}
				}
			}
		}

		private void Reader_EntryExtractionProgress(object? sender, SharpCompress.Common.ReaderExtractionEventArgs<SharpCompress.Common.IEntry> e)
		{
			MessageBox.Show("S");
			downloadProgressBar.Value = e.ReaderProgress.PercentageRead;
		}


		private void ResetProgressBar()
		{
			downloadProgressBar.Value = 0;
			downloadSpeedLabel.Content = string.Empty;
			downloadedSizeLabel.Content = string.Empty;
		}

		private void UpdateProgressBar(long bytesRead, long totalBytes)
		{
			// Обновить ProgressBar с текущим прогрессом загрузки
			downloadProgressBar.Value = (double)bytesRead / totalBytes * 100;

			// Рассчитать скорость загрузки
			double elapsedTimeInSeconds = stopwatch.Elapsed.TotalSeconds;
			double downloadSpeed = bytesRead / elapsedTimeInSeconds / 1024; // в килобайтах в секунду

			// Обновить метку скорости загрузки
			downloadSpeedLabel.Content = $"Скорость: {downloadSpeed:F2} KB/s";

			// Обновить метку скачанного размера и общего размера файла
			downloadedSizeLabel.Content = $"Скачано: {bytesRead / (1024 * 1024):F2} MB / {totalBytes / (1024 * 1024):F2} MB";
		}
	}
}
