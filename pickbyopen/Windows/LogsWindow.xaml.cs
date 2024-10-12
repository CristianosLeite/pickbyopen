﻿using Pickbyopen.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using Pickbyopen.Models;

namespace Pickbyopen.Windows
{
    /// <summary>
    /// Lógica interna para LogsWindow.xaml
    /// </summary>
    public partial class LogsWindow : Window
    {
        private readonly Db db = new(DatabaseConfig.ConnectionString!);
        public ObservableCollection<Log> Logs { get; set; }

        public LogsWindow()
        {
            InitializeComponent();

            dgLogs.AutoGenerateColumns = false;
            dgLogs.CanUserAddRows = false;
            dgLogs.CanUserDeleteRows = false;
            dgLogs.CanUserReorderColumns = false;
            dgLogs.CanUserResizeColumns = false;
            dgLogs.CanUserResizeRows = false;
            dgLogs.CanUserSortColumns = false;

            dgLogs.Columns.Add(new DataGridTextColumn
            {
                Header = "Data e hora",
                Binding = new Binding("CreatedAt"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            });

            dgLogs.Columns.Add(new DataGridTextColumn
            {
                Header = "Evento",
                Binding = new Binding("Event"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            });

            dgLogs.Columns.Add(new DataGridTextColumn
            {
                Header = "Alvo",
                Binding = new Binding("Target"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            });

            Logs = [];
            DataContext = this;
            LoadLogs();
        }

        private async void LoadLogs()
        {
            var logs = await db.LoadLogs();
            logs.Sort((a, b) => DateTime.Compare(b.CreatedAt, a.CreatedAt));
            foreach (var log in logs)
            {
                Logs.Add(log);
            }
        }
    }
}
