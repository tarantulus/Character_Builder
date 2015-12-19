﻿using System;
using System.Collections.Generic;
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
using DnDSupportTypes;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;

namespace Character_Builder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void main_click(object sender, RoutedEventArgs e)
        {
            var sw = new Stopwatch();
            sw.Start();
            
            blog($"starting: {sw.ElapsedMilliseconds} ms");
            
            if (!DnDCore.loaded)
            {
                log("Core data not loaded, try again later");
                return;
            }

            int numChars = 1000, current = 0;
            int threads = Environment.ProcessorCount - 1 < numChars ? Environment.ProcessorCount - 1 : numChars;
            Task[] producers = new Task[threads];
            string result = "";

            blog(threads.ToString());

            using (BlockingCollection<DnDCharacter> party = new BlockingCollection<DnDCharacter>())
            {
                //spin up multiple producer threads
                for (int i = 0; i < producers.Length; i++)
                {
                    producers[i] = Task.Run(() =>
                    {
                        while (current < numChars)
                        {
                            Interlocked.Increment(ref current);
                            party.TryAdd(new DnDCharacter());
                        }

                    });
                }

                //spin up simgle consumer thread
                var consumer = Task.Run(() =>
                {
                    while (!party.IsCompleted)
                    {
                        DnDCharacter temp;
                        while (party.TryTake(out temp))
                        {
                            result += $"{temp.ToString()}\n";
                        }
                    }
                });

                Task.WaitAll(producers);
                party.CompleteAdding();
                consumer.Wait();

                log(result);
            }

            //log("\n+++ Races +++\n");
            //foreach (var thing in DnDCore.Races)
            //{
            //    log(thing.ToString());
            //}

            //log("\n+++ Classes +++\n");
            //foreach (var thing in DnDCore.Classes)
            //{
            //    string testing = JsonConvert.SerializeObject(thing);
            //    log(testing);
            //}

            //log("\n+++ Features +++\n");
            //foreach (var thing in DnDCore.Features)
            //{
            //    log(thing.ToString());
            //}
            sw.Stop();
            blog($"complate: {sw.ElapsedMilliseconds} ms");
        }

        private void log(string message)
        {
            output_1.Text += $"{message}\n";
        }

        private void blog(string message)
        {
            output_2.Text += $"{message}\n";
        }

        private void clear_click(object sender, RoutedEventArgs e)
        {
            output_1.Text = "";
        }

        private void exit_click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bool pointless = DnDCore.loaded;
        }
    }
}
