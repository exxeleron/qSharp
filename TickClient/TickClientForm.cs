//
//   Copyright (c) 2011-2014 Exxeleron GmbH
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace qSharp.Sample
{
    public partial class TickClientForm : Form
    {
        QCallbackConnection q;

        public TickClientForm()
        {
            InitializeComponent();
        }

        private void subscribeBtn_Click(object sender, EventArgs e)
        {
            if (q == null)
            {
                string[] conn = qhostTB.Text.Split(':');
                q = new QCallbackConnection(host: conn.Length >= 1 ? conn[0] : "localhost",
                                            port: conn.Length >= 2 ? int.Parse(conn[1]) : 5010);

                try
                {
                    q.Open();
                    QTable model = (QTable)((object[])q.Sync(".u.sub", qtableTB.Text, ""))[1];
                    data.Columns.Clear();
                    foreach(string column in model.Columns)
                    {
                        data.Columns.Add(column);
                    }
                    
                    q.DataReceived += OnData;
                }
                catch (Exception e1)
                {
                    Console.Error.WriteLine(e1);
                    Console.ReadLine();
                    q.Close();
                }
            }
            q.StartListener();
        }

        private void unsubscribeBtn_Click(object sender, EventArgs e)
        {
            if (q != null)
            {
                q.StopListener();
            }
        }

        private void OnData(object sender, QMessageEvent message)
        {
            Object[] list = message.Message.Data as object[];
            if (list != null && list.Length == 3 && list[0].Equals("upd") && list[2] is QTable)
            {
                QTable table = list[2] as QTable;
                string[] buffer = new string[table.ColumnsCount];
                object item;
                foreach (QTable.Row row in table)
                {
                    for (int i = 0; i < table.ColumnsCount; i++)
                    {
                        item = row[i];
                        buffer[i] = item != null ? item.ToString() : "";
                    }

                    this.AddItem(buffer);
                }
            }
        }

        delegate void AddItemCallback(string[] item);

        private void AddItem(string[] item)
        {
            if (this.data.InvokeRequired)
            {
                AddItemCallback d = new AddItemCallback(AddItem);
                this.Invoke(d, new object[] { item });
            }
            else
            {
                data.Items.Insert(0, new ListViewItem(item));
            }
        }
    }
}
