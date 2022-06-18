using Microsoft.Win32;
using MoreLinq;
using Nito.AsyncEx.Synchronous;
using Normalization;
using Python.Included;
using Python.Runtime;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace DiplomaClientApplication
{
    public partial class MainWindow : Window
    {
        private static List<string> nouns = new List<string>() {"борьба", "внимание", "вопрос", "гость", "движение",
        "дело", "дружба", "задача", "здоровье", "значение", "идея", "интерес", "искусство", "кризис", "место",
        "мысль", "образование", "обстановка", "особенность", "очередь", "ошибка", "положение", "порядок",
        "пример", "проблема", "путь", "связь", "сила", "урок", "чувство"};

        private CoSyCo cosyco = new();
        private CrossLexica crossLexica = new();
        private Dictionary<string, string> perfectInitialVerbsDict;

        private Dictionary<VerbWithFrequencyInfo, HashSet<VerbWithFrequencyInfo>> cosycoDictionaryNormForms;
        private Dictionary<VerbWithFrequencyInfo, HashSet<VerbWithFrequencyInfo>> cosycoResultSet;

        private Dictionary<string, List<Pair<VerbWithFrequencyInfo,HashSet<VerbWithFrequencyInfo>>>> savedWords = new();


        private int cnt = 0;

        public MainWindow()
        {
            InitializeComponent();        
            nounDropDownList.ItemsSource = nouns;

            Installer.SetupPython().WaitWithoutException();
            PythonEngine.Initialize();  
            
            foreach(var noun in nouns)
            {
                savedWords.Add(noun, new List<Pair<VerbWithFrequencyInfo, HashSet<VerbWithFrequencyInfo>>>());
            }
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {

            dataTable.Items.Clear();
            dataTable.Items.Refresh();
            progressBar.Value = 0;

            var columns = dataTable.Columns;

            var noun = nounDropDownList.SelectedValue.ToString().ToUpper();


            perfectInitialVerbsDict = InitialPerfectVerbs.ReadVerbPairsFromCsv(@"C:\Users\dasha\Desktop\Normalization\PerfectVerbsPairs\perfectVerbs.csv");

            var crossLexicaPath = @"C:\Users\dasha\Desktop\Normalization\CrossLex\InitialFiles\" + noun + ".txt";

            var tst = MorphAnalyzer.Instance;


            await cosyco.Load(noun);
            progressBar.Value += 5;
            crossLexica.Load(crossLexicaPath);
            progressBar.Value += 5;

            var normalizedFormsCoSyCo = await Task.Run<Dictionary<VerbWithFrequencyInfo, HashSet<VerbWithFrequencyInfo>>>(() => cosyco.NormalizeVerbs(perfectInitialVerbsDict));
            progressBar.Value += 20;
            var cosycoWithoutPreposition = await Task.Run<Dictionary<VerbWithFrequencyInfo, HashSet<VerbWithFrequencyInfo>>>(() => normalizedFormsCoSyCo.CompressVervsWithoutPreposition());
            progressBar.Value += 20;

            cosycoDictionaryNormForms = normalizedFormsCoSyCo.ToDictionary(x => x.Key, x => x.Value);
            cosycoResultSet = cosycoWithoutPreposition.ToDictionary(x => x.Key, x => x.Value);

            var normalizedFromsCrossLexica = await Task.Run<Dictionary<VerbInfo, HashSet<VerbInfo>>>(() => crossLexica.NormalizeVerbs(perfectInitialVerbsDict));
            progressBar.Value += 20;
            var crossLexicaWithoutPreposition = await Task.Run<Dictionary<VerbInfo, HashSet<VerbInfo>>>(() => normalizedFromsCrossLexica.CompressVervsWithoutPreposition());
            progressBar.Value += 20;

            var difference = await Task.Run<IEnumerable<VerbWithFrequencyInfo>>(() => Difference.GetDifferenceOfCoSyCoCrossLexica(cosycoWithoutPreposition.Keys, crossLexicaWithoutPreposition.Keys).Where(x => x.LogDice() > 0)
               .OrderByDescending(x => x.LogDice()));
            progressBar.Value += 10;


            Dispatcher.Invoke(() =>
            {
                foreach (var comb in difference)
                {
                    cnt += cosycoWithoutPreposition[comb].Select(x => normalizedFormsCoSyCo[x].Count).Sum();

                    dataTable.Items.Add(new DataGridItemsWithCount()
                    {
                        Checked = false,
                        Verb = comb.Verb,
                        Prep = "",
                        LogDice = Math.Round(comb.LogDice(), 8),
                        MinSen = Math.Round(comb.MinSen(), 8),
                        Count = cosycoWithoutPreposition[comb].Select(x => normalizedFormsCoSyCo[x].Count).Sum(),
                        Selected = 0
                    });
                }
            });  
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var wind = new SelectValues(cosycoDictionaryNormForms, cosycoResultSet, savedWords,(sender as Button).Content.ToString(), nounDropDownList.SelectedValue.ToString());
            wind.dataSent += Cf_dataSent;
            wind.Show();         
        }

        private void Cf_dataSent(int cnt)
        {
            var item = dataTable.CurrentItem as DataGridItemsWithCount;
            item.Checked = cnt > 0;
            item.Selected = cnt;
            dataTable.Items.Refresh();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            var i = cnt;
            PythonEngine.Shutdown();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var noun = nounDropDownList.SelectedValue.ToString();

            var verbsCollection = savedWords[noun].Select(x => cosycoResultSet[x.First]).First()
                .SelectMany(x => cosycoDictionaryNormForms[x]).Intersect(savedWords[noun].Select(x => x.Second).First());
            
            
            var dlg = new SaveFileDialog();
            if(dlg.ShowDialog() == true)
            {
                var csvFile = CsvSerializer.SerializeToCsv(verbsCollection);
                File.WriteAllText(dlg.FileName, csvFile, Encoding.UTF8);
            }

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            foreach(DataGridItemsWithCount t in dataTable.Items)
            {
                if(t.Checked)
                {
                    var res = cosycoResultSet.Keys.Where(x => x.Verb.Equals(t.Verb)).First();
                    var ress = cosycoResultSet[res].SelectMany(x => cosycoDictionaryNormForms[x]).ToList();

 
                    var dlg = new SaveFileDialog();
                    if (dlg.ShowDialog() == true)
                    {
                        var json = JsonConvert.SerializeObject(ress);
                        File.WriteAllText(dlg.FileName, json, Encoding.UTF8);
                    }
                }
            }
        }
    }
}
