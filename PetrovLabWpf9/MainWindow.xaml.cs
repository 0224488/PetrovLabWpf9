using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

//  Доработать проект текстового редактора из задания 7,
//  добавив словарь ресурсов (в виде файла) со списками названий шрифтов и размеров.

namespace PetrovLabWpf9
{
    // Логика взаимодействия для MainWindow.xaml
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            lightMenuItem.IsChecked = true;
            
            
        }

        // обработка выбора названия шрифта
        private void ComboBoxFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string fontName = ((sender as ComboBox).SelectedItem as TextBlock).Text;
            if (textbox != null) 
            {
                textbox.FontFamily = new FontFamily(fontName);
            }
        }

        // обработка выбора размера шрифта
        private void ComboBoxFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            double fontSize = (double)(sender as ComboBox).SelectedItem;
            if(textbox != null)
            {
                textbox.FontSize = fontSize;
            }
        }

        // обработка выбора цвета текста
        private void RadioButtonColor_Checked(object sender, RoutedEventArgs e)
		{
            if(textbox != null)
            {
                var radioButton = sender as RadioButton;
                if(radioButton != null)
                {
                    textbox.Foreground = radioButton.Foreground;
                }
            }
        }

        // обработка выбора стиля шрифта (жирный, обычный)
        private void ToggleButtonBold_CheckedChanged(object sender, RoutedEventArgs e)
		{
            var toggleButton = sender as ToggleButton;
			if(toggleButton != null)
			{
				if(toggleButton.IsChecked == true)
					textbox.FontWeight = FontWeights.Bold;
                else
                    textbox.FontWeight = FontWeights.Normal;
            }
        }

        // обработка выбора стиля шрифта (курсив, обычный)
        private void ToggleButtonItalic_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as ToggleButton;
            if(toggleButton != null)
            {
                if(toggleButton.IsChecked == true)
                    textbox.FontStyle = FontStyles.Italic;
                else
                    textbox.FontStyle = FontStyles.Normal;
            }
        }

        // обработка выбора стиля шрифта (подчеркнутый, обычный)
        private void ToggleButtonUnderline_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as ToggleButton;
            if(toggleButton != null)
            {
                if(toggleButton.IsChecked == true)
                    textbox.TextDecorations = TextDecorations.Underline;
                else
                    textbox.TextDecorations = null;
            }
        }

        // обработка команды Открыть
        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)

        {
            var dlg = new OpenFileDialog();
			if(dlg.ShowDialog() == true)
			{
                // читаем документ из выбранного файла
                var jsonService = new JsonFileService();
                var document = jsonService.Open(dlg.FileName);

                InitializeDocument(document);
            }
        }

        // обработка команды Сохранить
        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            if(saveFileDialog.ShowDialog() == true)
            {
                // создаем документ
                var document = new Document()
                {
					Text = textbox.Text,
					FontName = textbox.FontFamily.Source,
					FontSize = textbox.FontSize,
					IsBold = textbox.FontWeight == FontWeights.Bold,
                    IsItalic = textbox.FontStyle == FontStyles.Italic,
                    IsUnderline = textbox.TextDecorations == TextDecorations.Underline,
					Color = (textbox.Foreground as SolidColorBrush).Color
				};
                // сериализуем его в файл
                var jsonService = new JsonFileService();
                jsonService.Save(saveFileDialog.FileName, document);
            }
        }

        // обработка команды Закрыть
        private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(MessageBox.Show("Сохранить изменения в документ?", "Сохранение", 
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.OK)
			{
                Save_Executed(null, null);
			}
            ResetValues();
        }

        // выставление свойств документа
        private void InitializeDocument(Document document)
		{
            textbox.Text = document.Text;
			comboBoxFontName.SelectedItem = comboBoxFontName.Items.OfType<TextBlock>()
                .FirstOrDefault(t => t.Text.Equals(document.FontName.ToString(), StringComparison.InvariantCultureIgnoreCase));
            comboBoxFontSize.SelectedItem = comboBoxFontSize.Items.OfType<double>()
                .FirstOrDefault(t => t == document.FontSize);
            toggleButtonBold.IsChecked = document.IsBold;
            toggleButtonItalic.IsChecked = document.IsItalic;
            toggleButtonUnderline.IsChecked = document.IsUnderline;
            if(document.Color == Colors.Black)
            {
                radioButtonBlack.IsChecked = true;
            }
            else if(document.Color == Colors.Red)
            {
                radioButtonRed.IsChecked = true;
            }
        }

        // выставление исходных свойств нового документа
        private void ResetValues()
		{
            textbox.Text = string.Empty;
            comboBoxFontName.SelectedIndex = 0;
            comboBoxFontSize.SelectedIndex = 3;
            toggleButtonBold.IsChecked = false;
            toggleButtonItalic.IsChecked = false;
            toggleButtonUnderline.IsChecked = false;
            radioButtonBlack.IsChecked = true;
        }

        public class JsonFileService
        {
            // чтение документа из файла с указанным именем
            public Document Open(string fileName)
            {
                Document document = null;
                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                    WriteIndented = true
                };
                
                using(StreamReader reader = new StreamReader(fileName))
                {
					var jsonString = reader.ReadToEnd();
					document = JsonSerializer.Deserialize(jsonString, typeof(Document), options) as Document;
				}
                return document;
            }

            // запись документа в файл с указанным именем
            public void Save(string fileName, Document document)
            {
                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                    WriteIndented = true
                };
                var jsonString = JsonSerializer.Serialize(document, options);
                using(StreamWriter writer = new StreamWriter(fileName))
                {
                    writer.Write(jsonString);
                }
            }
        }

        public class Document
        {
            public string Text { get; set; }
            public double FontSize{ get; set; }
            public string FontName { get; set; }
            public bool IsBold { get; set; }
            public bool IsItalic { get; set; }
            public bool IsUnderline { get; set; }
            public Color Color { get; set; }
        }

		private void MenuItem_Checked(object sender, RoutedEventArgs e)
		{
            var menuItem = sender as MenuItem;
            if(menuItem.IsChecked)
            {
                var style = string.Empty;
                if((sender as MenuItem) == lightMenuItem)
                {
                    darkMenuItem.IsChecked = false;
                    style = "light";
                }
                else if((sender as MenuItem) == darkMenuItem)
                {
                    lightMenuItem.IsChecked = false;
                    style = "dark";
                }
                if(!string.IsNullOrEmpty(style))
                    ThemeChange(style);
            }
        }

        private void ThemeChange(string style)
        {
            // определяем путь к файлу ресурсов
            var uri = new Uri($"/Themes/{style}.xaml", UriKind.Relative);
            var dictUri = new Uri("Dictionary1.xaml", UriKind.Relative);
            // загружаем словарь ресурсов
            ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
            resourceDict.Source = uri;
            // очищаем коллекцию ресурсов приложения
            Application.Current.Resources.Clear();
            var dict = Application.Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Source == dictUri);
            Application.Current.Resources.MergedDictionaries.Clear();
            // добавляем загруженный словарь ресурсов
            if(dict != null)
                Application.Current.Resources.MergedDictionaries.Add(dict);
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);
        }
	}
}
