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
using Newtonsoft.Json.Linq;
using DiplomaClientApplication.Infrastructure.JsonObjects;
using System.Windows.Threading;
using Microsoft.Extensions.Options;
using DiplomaClientApplication.Infrastructure;

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

        private readonly MyConfig config;

        public MainWindow(IOptions<MyConfig> config)
        {
            try
            {
                //Console.WriteLine("Config");
                this.config = config.Value;

                //Console.WriteLine("Start initializing components in a window");

                InitializeComponent();

                //Console.WriteLine("End initializing");

                nounDropDownList.ItemsSource = nouns;                

                //Console.WriteLine("Start to Set environment");

                //Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", this.config.PythonDll);

                Installer.InstallPath = this.config.InstallingPipPath;
                Installer.InstallDirectory = this.config.InstallingPipDirectory;

                //Console.WriteLine("end to set environment");

                //Console.WriteLine("SetUp python");

                Installer.SetupPython();

                //Console.WriteLine("SetUp finished");

                //Console.WriteLine("Engine initialization started");

                PythonEngine.Initialize();

                //Console.WriteLine("Engine initialization finished");

                var initialize = MorphAnalyzer.Instance;

                foreach (var noun in nouns)
                {
                    savedWords.Add(noun, new List<Pair<VerbWithFrequencyInfo, HashSet<VerbWithFrequencyInfo>>>());
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private async void ShowCombinationsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowCombinationsButton.IsEnabled = false;

            dataTable.Items.Clear();
            dataTable.Items.Refresh();

            progressBar.Value = 0;

            try
            {

                var noun = nounDropDownList.SelectedValue.ToString().ToUpper();

                perfectInitialVerbsDict = InitialPerfectVerbs.ReadVerbPairsFromCsv(config.PathToDictionary);

                var crossLexicaPath = config.PathToCrossLexica + noun + ".txt";

                await cosyco.Load(noun);


                progressBar.Value += 5;

                await Task.Run(() => crossLexica.Load(crossLexicaPath));

                progressBar.Value += 5;

                DoEvents();

                var normalizedFormsCoSyCo = await Task.Run<Dictionary<VerbWithFrequencyInfo, HashSet<VerbWithFrequencyInfo>>>(() => cosyco.NormalizeVerbs(perfectInitialVerbsDict));

                progressBar.Value += 20;

                var cosycoWithoutPreposition = await Task.Run<Dictionary<VerbWithFrequencyInfo, HashSet<VerbWithFrequencyInfo>>>(() => normalizedFormsCoSyCo.CompressVerbsWithoutPreposition());

                progressBar.Value += 20;

                cosycoDictionaryNormForms = normalizedFormsCoSyCo.ToDictionary(x => x.Key, x => x.Value);
                cosycoResultSet = cosycoWithoutPreposition.ToDictionary(x => x.Key, x => x.Value);

                var normalizedFromsCrossLexica = await Task.Run<Dictionary<VerbInfo, HashSet<VerbInfo>>>(() => crossLexica.NormalizeVerbs(perfectInitialVerbsDict));

                progressBar.Value += 20;

                var crossLexicaWithoutPreposition = await Task.Run<Dictionary<VerbInfo, HashSet<VerbInfo>>>(() => normalizedFromsCrossLexica.CompressVerbsWithoutPreposition());

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
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте подключение к сети.", "Error", MessageBoxButton.OK);
            }
            finally
            {
                ShowCombinationsButton.IsEnabled = true;
                progressBar.Value = 0;
            }

        }

        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                  new Action(delegate { }));
        }

        private void SelectedVerbButton_Click(object sender, RoutedEventArgs e)
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
            MorphAnalyzer.Instance.Dispose();
        }

        private void SaveToCsvButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var noun = nounDropDownList.SelectedValue.ToString();

                var selectedItems = savedWords[noun].SelectMany(x => x.Second);
                var allItems = savedWords[noun].SelectMany(x => cosycoResultSet[x.First]).SelectMany(x => cosycoDictionaryNormForms[x]);

                var intersection = allItems.Intersect<VerbWithFrequencyInfo>(selectedItems, new VerbsComparer()).ToList();

                var dlg = new SaveFileDialog();

                if (dlg.ShowDialog() == true)
                {
                    var csvFile = CsvSerializer.SerializeToCsv(intersection);
                    dlg.DefaultExt = "csv";
                    File.WriteAllText(dlg.FileName, csvFile, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Выберете словосочетания", "Error", MessageBoxButton.OK);
            }

        }

        private void FillNode<T>(T node, VerbWithFrequencyInfo verb)
            where T : VerbNode
        {
            node.Verb = verb.Verb;
            node.Prep = verb.Prep;
            node.NounFrequency = verb.NounFrequency;
            node.VerbFrequency = verb.VerbFrequency;
            node.CombinationFrequency = verb.CombinationFrequency;
            node.LogDice = Math.Round(verb.LogDice(),8);
            node.MinimumSensitivity = Math.Round(verb.MinSen());
        }


        private void SaveToJsonButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Title = "Выберите директорию";

            string dummyFileName = "Save Here";

            dlg.FileName = dummyFileName;

            if (dataTable.Items.IsEmpty)
            {
                MessageBox.Show("Выберите слово и словосочетание", "Error", MessageBoxButton.OK);
            }
            else if (dlg.ShowDialog() == true)
            {

                foreach (DataGridItemsWithCount t in dataTable.Items)
                {
                    if (t.Checked)
                    {
                        var selectedKey = cosycoResultSet.Keys.Where(x => x.Verb.Equals(t.Verb)).First();

                        var treeRoot = new VerbTreeJson<VerbTreeJson<VerbNode>>();

                        FillNode<VerbTreeJson<VerbTreeJson<VerbNode>>>(treeRoot, selectedKey);

                        treeRoot.Children = new List<VerbTreeJson<VerbNode>>();

                        var index = 0;

                        foreach (var normilizedVerb in cosycoResultSet[selectedKey])
                        {
                            treeRoot.Children.Add(new VerbTreeJson<VerbNode>());

                            FillNode<VerbTreeJson<VerbNode>>(treeRoot.Children[index], normilizedVerb);

                            var j = 0;

                            treeRoot.Children[index].Children = new List<VerbNode>();

                            foreach (var initialVerb in cosycoDictionaryNormForms[cosycoResultSet[selectedKey].First()])
                            {

                                treeRoot.Children[index].Children.Add(new VerbNode());

                                FillNode<VerbNode>(treeRoot.Children[index].Children[j], initialVerb);

                                j++;
                            }

                            index++;

                        }

                        var path = System.IO.Path.GetDirectoryName(dlg.FileName);

                        var json = JsonConvert.SerializeObject(treeRoot, Formatting.Indented);

                        File.WriteAllText(path + @"\" + selectedKey.Verb, json, Encoding.UTF8);
                        
                    }
                }
            }
        }
    }
}
