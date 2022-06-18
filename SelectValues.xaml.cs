using Normalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DiplomaClientApplication
{
    public delegate void DataSentHandler(int count);

    public partial class SelectValues : Window
    {
        private Dictionary<string, List<Pair<VerbWithFrequencyInfo, HashSet<VerbWithFrequencyInfo>>>> selectedWords;

        //private static readonly ObjectCache cache = MemoryCache.Default;

        public event DataSentHandler dataSent;

        private int count = 0;

        private string noun;

        private VerbWithFrequencyInfo verb;

        //private SelectValues()
        //{
        //    if (!cache.Contains(nameof(selectedWords)))
        //    {
        //        cache.Add(nameof(selectedWords), new HashSet<string>(), new CacheItemPolicy());                 
        //    }
        //}

        public SelectValues(Dictionary<VerbWithFrequencyInfo, HashSet<VerbWithFrequencyInfo>> cosycoDictionaryNormForms,
            Dictionary<VerbWithFrequencyInfo, HashSet<VerbWithFrequencyInfo>> cosycoResultSet,
            Dictionary<string, List<Pair<VerbWithFrequencyInfo, HashSet<VerbWithFrequencyInfo>>>> savedWords,
            string selectedVerb, string selectedNoun) 
        {
            this.Title = selectedNoun + " " + selectedVerb;
            InitializeComponent();

            verb = cosycoResultSet.Keys.Where(x => x.Verb.Equals(selectedVerb)).First();
            var ress = cosycoResultSet[verb].SelectMany(x => cosycoDictionaryNormForms[x]).ToList();

            //selectedWords = cache[nameof(selectedWords)] as HashSet<string>;

            foreach (var e in ress)
            {
                var newItem = new DataGridItems()
                {
                    Verb = e.Verb,
                    Prep = e.Prep,
                    LogDice = Math.Round(e.LogDice(), 8),
                    MinSen = Math.Round(e.MinSen(), 8)
                };

                var k = savedWords[selectedNoun].Where(x => x.First.Equals(verb)).FirstOrDefault();

                if (k == null)
                {
                    savedWords[selectedNoun].Add(new Pair<VerbWithFrequencyInfo, HashSet<VerbWithFrequencyInfo>>()
                    {
                        First = verb,
                        Second = new HashSet<VerbWithFrequencyInfo>(new VerbsComparer())
                    });
                }
                else
                {
                    if (k.Second.Contains(e)) { newItem.Checked = true; }
                }


                selectedWords = savedWords;

                noun = selectedNoun;

                dataGrid.Items.Add(newItem);

            }            
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //selectedWords = cache[nameof(selectedWords)] as HashSet<string>;
            var dataContext = (sender as CheckBox)?.DataContext as DataGridItems;

            var k = selectedWords[noun].Where(x => x.First.Equals(verb)).FirstOrDefault();

            var selectVerb = new VerbWithFrequencyInfo() { Verb = dataContext.Verb, Prep = dataContext.Prep };

            if (!k.Second.Contains(selectVerb))
            {
                k.Second.Add(selectVerb);
                count++;
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //selectedWords = cache[nameof(selectedWords)] as HashSet<string>;
            var dataContext = (sender as CheckBox)?.DataContext as DataGridItems;

            var k = selectedWords[noun].Where(x => x.First.Equals(verb)).FirstOrDefault();

            var selectVerb = new VerbWithFrequencyInfo() { Verb = dataContext.Verb, Prep = dataContext.Prep };

            if (k.Second.Contains(selectVerb))
            {
                k.Second.Remove(selectVerb);
                count--;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.dataSent(count);
        }
    }
}
