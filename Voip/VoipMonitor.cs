using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using System.Windows.Forms.DataVisualization.Charting;

namespace Voip
{
    public partial class VoipMonitor : MaterialForm
    {
        SimulationParamters simulationParamters;
        Thread thread;
        bool isRunning = false;
        int runningCount = 1;
        private int gapTime = 1;
        private int successfulPredictionCounter = 0;
        private double successRate = 0;
        private bool firstAll = true;
        private bool isAll = false;
        private int count = 1;
        private int statisticCount = 0, sum1 = 0, sum2 = 0, sum3 = 0, sum4 = 0;
        private int currenPCAsample=1;
        private Random rnd = new Random();
        private List<Color> colorList=new List<Color>();
        private Color color;
        public VoipMonitor()
        {
            InitializeComponent();
            initializeSummaryDatGridView();
            MaterialSkinManager materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;

            // Configure color schema
            materialSkinManager.ColorScheme = new ColorScheme(
                Primary.Grey900, Primary.Grey900,
                Primary.Grey900, Accent.LightBlue200,
                TextShade.WHITE
            );
            simulationParamters = new SimulationParamters();
            simulationParamters.features = new List<string>();
            colorList.Add(Color.Red);
            colorList.Add(Color.OrangeRed);
            colorList.Add(Color.Orange);
            colorList.Add(Color.Gold);
            colorList.Add(Color.YellowGreen);
            colorList.Add(Color.LawnGreen);
            colorList.Add(Color.GreenYellow);
            colorList.Add(Color.Green);
            colorList.Add(Color.RoyalBlue);
            colorList.Add(Color.SkyBlue);
            chart1.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.White;
            chart1.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.White;

        }

        private void initializeSummaryDatGridView()
        {
            summaryDatagridview.Rows.Add();
            summaryDatagridview.Rows.Add();
            summaryDatagridview.Rows.Add();
            summaryDatagridview.Rows.Add();
            summaryDatagridview.Rows.Add();
            foreach (DataGridViewRow row in summaryDatagridview.Rows)
            {
                row.Height = 50;
            }
            summaryDatagridview[0, 0].Value = "Bad";
            summaryDatagridview[0, 1].Value = "Fair";
            summaryDatagridview[0, 2].Value = "Good";
            summaryDatagridview[0, 3].Value = "Unknown";
            summaryDatagridview[0, 4].Value = "Predict";
            summaryDatagridview.Columns[0].DefaultCellStyle.BackColor = Color.FromArgb(50, 50, 50);
            summaryDatagridview[0, 0].Style.ForeColor = Color.Red;
            summaryDatagridview[0, 1].Style.ForeColor = Color.Gold;
            summaryDatagridview[0, 2].Style.ForeColor = Color.SpringGreen;
            summaryDatagridview[0, 3].Style.ForeColor = Color.DodgerBlue;
            summaryDatagridview.CurrentCell = summaryDatagridview[0, 4];
            summaryDatagridview.Refresh();

            DataGridViewColumn dataGridViewColumn = summaryDatagridview.Columns[1];
            dataGridViewColumn.HeaderCell.Style.ForeColor = Color.Red;
            dataGridViewColumn = summaryDatagridview.Columns[2];
            dataGridViewColumn.HeaderCell.Style.ForeColor = Color.Gold;
            dataGridViewColumn = summaryDatagridview.Columns[3];
            dataGridViewColumn.HeaderCell.Style.ForeColor = Color.SpringGreen;
            dataGridViewColumn = summaryDatagridview.Columns[4];
            dataGridViewColumn.HeaderCell.Style.ForeColor = Color.DodgerBlue;
        }

        private void runSimulationButton_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            thread = new Thread(() => handleStartSimulationClicked(dataGridView1.Rows[0]));
            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void handleStartSimulationClicked(DataGridViewRow initialRow)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)(() =>
                {
                    this.Cursor = Cursors.WaitCursor;
                }));
            }
            else
            {
                this.Cursor = Cursors.WaitCursor;
            }
            if (!isRunning)
            {
                isRunning = true;
                firstAll = true;
                ClearStatistics();
                runSimulationButton.BackColor = Color.PowderBlue;
                stopSimulation.BackColor = Color.Red;
                count = 1;
                //Print the simulation data to file and start the python process
                if (!isAll)
                {
                    WriteConfigToFile(@"C:\Users\ramif\Desktop\Voip\Params.csv");
                    
                    ExecutePythonCode("pythonApp.py");
                    ShowResults(initialRow);
                    ShowStatistics(1);
                    isRunning = false;
                }
                else
                {
                    for (int i = 1; i < 11; i++)
                    {
                        color = colorList[i-1];
                        simulationParamters.numOfSamples = i;
                        WriteConfigToFile(@"C:\Users\ramif\Desktop\Voip\Params.csv");
                        ExecutePythonCode("pythonApp.py");
                        ShowResults(initialRow);
                        ShowStatistics(i);
                        isRunning = false;
                    }
                    simulationParamters.numOfSamples = -1;
                    firstAll = true;
                }
                stopSimulation.BackColor = Color.RosyBrown;
                runSimulationButton.BackColor = Color.DodgerBlue;
            }
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)(() =>
                {
                    this.Cursor = Cursors.Default;
                }));
            }
            else
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void addRow(DataGridViewRow row)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)(() =>
                {
                    dataGridView1.Rows.Add(row);
                    dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.Rows.Count - 1;
                }));
            }
            else
            {
                dataGridView1.Rows.Add(row);
                dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.Rows.Count - 1;
            }
        }

        private void stopSimulation_Click(object sender, EventArgs e)
        {
            if (isRunning)
            {
                this.Cursor = Cursors.WaitCursor;
                isRunning = false;
                stopSimulation.BackColor = Color.RosyBrown;
                runSimulationButton.BackColor = Color.DodgerBlue;
                thread.Abort();
                this.Cursor = Cursors.Default;

            }
        }

        private void NumOfSamples_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (NumOfSamples.Text.ToString() != "ALL")
            {
                simulationParamters.numOfSamples = int.Parse(NumOfSamples.Text.ToString());
                isAll = false;
            }
            else
            {
                simulationParamters.numOfSamples = -1;//-1==ALL
                isAll = true;
            }
        }

        private void numOfCallsTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (numOfCallsTextBox.Text.ToString().Length != 0)
                    simulationParamters.numOfCalls = int.Parse(numOfCallsTextBox.Text.ToString());
                else
                    simulationParamters.numOfCalls = 0;
            }
            catch { numOfCallsTextBox.Text = ""; }
        }

        private void Algorithem_SelectedIndexChanged(object sender, EventArgs e)
        {
            simulationParamters.choosenAlgorithem = Algorithem.Text.ToString();
        }

        private void Jitter_CheckedChanged(object sender, EventArgs e)
        {
            if (simulationParamters.features.Contains("Jitter"))
                simulationParamters.features.Remove("Jitter");
            else
                simulationParamters.features.Add("Jitter");
        }

        private void PacketLoss_CheckedChanged(object sender, EventArgs e)
        {
            if (simulationParamters.features.Contains("PacketLoss"))
                simulationParamters.features.Remove("PacketLoss");
            else
                simulationParamters.features.Add("PacketLoss");
        }

        private void Delay_CheckedChanged(object sender, EventArgs e)
        {
            if (simulationParamters.features.Contains("Delay"))
                simulationParamters.features.Remove("Delay");
            else
                simulationParamters.features.Add("Delay");
        }

        private void Echo_CheckedChanged(object sender, EventArgs e)
        {
            if (simulationParamters.features.Contains("Echo"))
                simulationParamters.features.Remove("Echo");
            else
                simulationParamters.features.Add("Echo");
        }
        private void MOS_CheckedChanged(object sender, EventArgs e)
        {
            if (simulationParamters.features.Contains("MOS"))
                simulationParamters.features.Remove("MOS");
            else
                simulationParamters.features.Add("MOS");
        }
        private void VQ_COLOR_CheckedChanged(object sender, EventArgs e)
        {
            if (simulationParamters.features.Contains("VQ_Color"))
                simulationParamters.features.Remove("VQ_Color");
            else
                simulationParamters.features.Add("VQ_Color");
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            simulationParamters.choosenAlgorithem = Algorithem.Text.ToString();
        }

        private void RunningMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            simulationParamters.runnningMode = Running.Text.ToString();
        }
        public void WriteConfigToFile(string file)
        {
            if (!File.Exists(file))
            {
                using (var stream = File.Create(file)) { }
            }
            string line = "parameters,values";
            using (var stream = File.CreateText(file))
            {
                stream.WriteLine(line);
                try
                {
                    line = "choosenAlgoritem," + simulationParamters.choosenAlgorithem.ToString();
                    stream.WriteLine(line);
                    line = "VQ ," + simulationParamters.features.Contains("VQ_Color").ToString();
                    stream.WriteLine(line);
                    line = "MOS ," + simulationParamters.features.Contains("MOS").ToString();
                    stream.WriteLine(line);
                    line = "JITTER ," + simulationParamters.features.Contains("Jitter").ToString();
                    stream.WriteLine(line);
                    line = "PLOSS ," + simulationParamters.features.Contains("PacketLoss").ToString();
                    stream.WriteLine(line);
                    line = "DELAY ," + simulationParamters.features.Contains("Delay").ToString();
                    stream.WriteLine(line);
                    line = "ECHO ," + simulationParamters.features.Contains("Echo").ToString();
                    stream.WriteLine(line);
                    line = "numOfCalls," + simulationParamters.numOfCalls.ToString();
                    stream.WriteLine(line);
                    line = "numOfSamples," + simulationParamters.numOfSamples.ToString();
                    stream.WriteLine(line);
                    line = "validationSize," + simulationParamters.validationPercent.ToString();
                    stream.WriteLine(line);
                    line = "speed," + simulationParamters.runnningMode.ToString();
                    stream.WriteLine(line);
                    line = "unknown," + simulationParamters.includeUnknown.ToString();
                    stream.WriteLine(line);

                }
                catch
                {
                    Console.WriteLine("Error while parsing data");
                }
            }
        }

        public void WritePCAConfigToFile(string file)
        {
            if (!File.Exists(file))
            {
                using (var stream = File.Create(file)) { }
            }
            string line = "parameters,values";
            using (var stream = File.CreateText(file))
            {
                stream.WriteLine(line);
                try
                {
                    line = "choosenAlgoritem," + simulationParamters.viewerAlgorithem.ToString();
                    stream.WriteLine(line);
                    line = "numOfCalls," + simulationParamters.viewerNumOfCalls.ToString();
                    stream.WriteLine(line);
                    line = "numOfSamples," + simulationParamters.viewerNumOfSamples.ToString();
                    stream.WriteLine(line);
                    line = "unknown," + simulationParamters.pcaIncludeUnknown.ToString();
                    stream.WriteLine(line);

                }
                catch
                {
                    Console.WriteLine("Error while parsing data");
                }
            }
        }
        public void WriteRobustConfigToFile(string file)
        {
            if (!File.Exists(file))
            {
                using (var stream = File.Create(file)) { }
            }
            string line = "parameters,values";
            using (var stream = File.CreateText(file))
            {
                stream.WriteLine(line);
                try
                {
                    line = "numOfCalls," + simulationParamters.robustNumOfCalls.ToString();
                    stream.WriteLine(line);
                    line = "numOfSamples," + simulationParamters.robustNumOfSamples.ToString();
                    stream.WriteLine(line);
                    line = "validationSize," + simulationParamters.robustValidationPercent.ToString();
                    stream.WriteLine(line);

                }
                catch
                {
                    Console.WriteLine("Error while parsing data");
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        public Color GetColor(string sColor)
        {
            if (sColor == "0")
                return Color.Red;
            if (sColor == "1")
                return Color.Yellow;
            if (sColor == "2")
                return Color.LawnGreen;
            return Color.Blue;

        }

        private void ValidationPercent_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (validationPercentTex.Text.ToString().Length != 0)
                    simulationParamters.validationPercent = double.Parse(validationPercentTex.Text.ToString());
                else
                    simulationParamters.validationPercent = 0;
            }
            catch { validationPercentTex.Text = ""; }
        }
        public void ExecutePythonCode(string filename)
        {
            Process p = new Process(); // create process (i.e., the python program
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = @"C:\Users\ramif\Anaconda3\python.exe";
            p.StartInfo.Arguments = @"C:\Users\ramif\Desktop\Voip\" + filename; // start the python program with two parameters
            p.Start(); // start the process (the python program)
            StreamReader s = p.StandardOutput;
            String output = s.ReadToEnd();
            string[] r = output.Split(new char[] { ' ', '\r', '\n' }); // get the parameter
            List<string> listOfParam = r.ToList();
            listOfParam.RemoveAll(str => String.IsNullOrEmpty(str));
            p.WaitForExit();
        }

        public void ShowResults(DataGridViewRow initialRow)
        {
             
            if (simulationParamters.runnningMode == "Medium")
                gapTime = 50;
            else if (simulationParamters.runnningMode == "Slow")
                gapTime = 100;
            using (StreamReader sr = new StreamReader(@"C:\Users\ramif\Desktop\Voip\exports\appResults.csv"))
            {
                string currentLine;
                if (!isAll)
                {
                    count = 1;
                    successfulPredictionCounter = 0;
                    addSerise("Run " + runningCount.ToString(),chart1,SeriesChartType.Line);
                    runningCount++;
                }
                else if (firstAll)
                {
                    count = 1;
                    successfulPredictionCounter = 0;
                    addSerise("Run " + runningCount.ToString(), chart1, SeriesChartType.Line);
                    runningCount++;
                    //firstAll = false;
                }
                // currentLine will be null when the StreamReader reaches the end of file
                while ((currentLine = sr.ReadLine()) != null)
                {
                    try
                    {
                        if (currentLine.Length == 3)
                        {

                            List<string> values = currentLine.Split(',').ToList<string>();
                            DataGridViewRow row = (DataGridViewRow)initialRow.Clone();
                            row.Cells[0].Value = count;
                            count++;
                            row.Cells[1].Style.BackColor = GetColor(values[1]);
                            row.Cells[2].Style.BackColor = GetColor(values[0]);
                            if (values[0] == values[1])
                            {
                                row.Cells[3].Value = "Yes";
                                row.Cells[3].Style.ForeColor = Color.Lime;
                                successfulPredictionCounter++;
                            }
                            else
                            {
                                row.Cells[3].Value = "No";
                                row.Cells[3].Style.ForeColor = Color.Red;
                            }
                            row.Cells[4].Value = simulationParamters.numOfSamples.ToString();
                            row.Height = 35;
                            //dataGridView1.Rows.Add(row);
                            addRow(row);
                            successRate = (double)successfulPredictionCounter / count;
                            if (this.chart1.InvokeRequired)
                            {
                                this.BeginInvoke((MethodInvoker)(() =>
                                {
                                    this.chart1.Series["Run " + chart1.Series.Count.ToString()].Points.AddXY(count, successRate);
                                    this.chart1.Series["Run " + chart1.Series.Count.ToString()].Points[this.chart1.Series["Run " + chart1.Series.Count.ToString()].Points.Count - 1].Color = color;
                                }));
                            }
                            else
                            {
                                this.chart1.Series["Run " + chart1.Series.Count.ToString()].Points.AddXY(count, successRate);
                                this.chart1.Series["Run " + chart1.Series.Count.ToString()].Points[this.chart1.Series["Run " + chart1.Series.Count.ToString()].Points.Count-1].Color= color; 
                            }

                        }
                    }
                    catch { };
                    Thread.Sleep(gapTime);
                }
                if (this.textBox1.InvokeRequired)
                {
                    this.BeginInvoke((MethodInvoker)(() =>
                    {
                        string success = Math.Round((successRate * 100), 2).ToString() + "%";
                        this.textBox1.Text = success;
                    }));
                }
                else
                {
                    string success = Math.Round((successRate * 100), 2).ToString() + "%";
                    this.textBox1.Text = success;
                }
            }
            if (!isAll)
            {
                firstAll = false;
                gapTime = 1;
                successfulPredictionCounter = 0;
                //successRate = 0;
            }


        }
        public void ShowStatistics(int index)
        {
            using (StreamReader sr = new StreamReader(@"C:\Users\ramif\Desktop\Voip\exports\statistics.csv"))
            {
                string currentLine;
                List<string> values;
                int statisticCount = 0;
                if (!isAll||index==1)
                {
                    sum1 = 0; sum2 = 0; sum3 = 0; sum4 = 0;
                }
                // currentLine will be null when the StreamReader reaches the end of file
                while ((currentLine = sr.ReadLine()) != null)
                {
                    values = currentLine.Split(',').ToList<string>();
                    if (statisticCount > 0 && statisticCount < 5)
                    {
                        if (!isAll || summaryDatagridview[1, statisticCount - 1].Value.ToString() == "")
                        {
                            summaryDatagridview[1, statisticCount - 1].Value = values[0];
                            sum1 += int.Parse(values[0]);
                            summaryDatagridview[2, statisticCount - 1].Value = values[1];
                            sum2 += int.Parse(values[1]);
                            summaryDatagridview[3, statisticCount - 1].Value = values[2];
                            sum3 += int.Parse(values[2]);
                            summaryDatagridview[4, statisticCount - 1].Value = values[3];
                            sum4 += int.Parse(values[3]);
                            summaryDatagridview[5, statisticCount - 1].Value = int.Parse(values[0]) + int.Parse(values[1]) + int.Parse(values[2]) + int.Parse(values[3]);
                        }
                        else
                        {
                            summaryDatagridview[1, statisticCount - 1].Value = int.Parse(summaryDatagridview[1, statisticCount - 1].Value.ToString()) + int.Parse(values[0]);
                            sum1 += int.Parse(values[0]);
                            summaryDatagridview[2, statisticCount - 1].Value = int.Parse(summaryDatagridview[2, statisticCount - 1].Value.ToString()) + int.Parse(values[1]);
                            sum2 += int.Parse(values[1]);
                            summaryDatagridview[3, statisticCount - 1].Value = int.Parse(summaryDatagridview[3, statisticCount - 1].Value.ToString()) + int.Parse(values[2]);
                            sum3 += int.Parse(values[2]);
                            summaryDatagridview[4, statisticCount - 1].Value = int.Parse(summaryDatagridview[4, statisticCount - 1].Value.ToString()) + int.Parse(values[3]);
                            sum4 += int.Parse(values[3]);
                            summaryDatagridview[5, statisticCount - 1].Value = int.Parse(summaryDatagridview[5, statisticCount - 1].Value.ToString()) + (int.Parse(values[0]) + int.Parse(values[1]) + int.Parse(values[2]) + int.Parse(values[3]));
                        }

                    }
                    statisticCount++;
                }
                summaryDatagridview[1, statisticCount - 3].Value = sum1;
                summaryDatagridview[2, statisticCount - 3].Value = sum2;
                summaryDatagridview[3, statisticCount - 3].Value = sum3;
                summaryDatagridview[4, statisticCount - 3].Value = sum4;
                double redPecentage = (double)Convert.ToInt32(summaryDatagridview[1, 0].Value)/ Convert.ToInt32(summaryDatagridview[5, 0].Value);
                if (this.textBox1.InvokeRequired)
                {
                    this.BeginInvoke((MethodInvoker)(() =>
                    {
                        string success = Math.Round((redPecentage * 100), 2).ToString() + "%";
                        this.textBox2.Text = success;
                    }));
                }
                else
                {
                    string success = Math.Round((redPecentage * 100), 2).ToString() + "%";
                    this.textBox2.Text = success;
                }
            }

        }
        public void addSerise(string name,Chart chart, SeriesChartType type)
        {
            if (chart.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)(() =>
                {
                    chart.Series.Add(name);
                    chart.Series[name].Color = Color.DeepSkyBlue;
                    chart.Series[name].BorderColor = Color.White;
                    chart.Series[name].ChartType = type;
                    chart.Series[name].ChartArea = "ChartArea1";
                }));
            }
            else
            {
                chart.Series.Add(name);
                chart.Series[name].Color = Color.DeepSkyBlue;
                chart.Series[name].ChartType = type;
                chart.Series[name].ChartArea = "ChartArea1";

            }

        }
        public void ClearStatistics()
        {
            for (int i = 0; i < 5; i++)
            {
                summaryDatagridview[1, i].Value = "";
                summaryDatagridview[2, i].Value = "";
                summaryDatagridview[3, i].Value = "";
                summaryDatagridview[4, i].Value = "";
                summaryDatagridview[5, i].Value = "";
            }
            if (this.textBox1.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)(() =>
                {
                    this.textBox1.Text = "";
                    this.textBox2.Text = "";
                }));
            }
            else
            {
                this.textBox1.Text = "";
            }
        }

        private void clearGraphs_Click(object sender, EventArgs e)
        {
            chart1.Series.Clear();
            runningCount = 1;
            firstAll = true;
            count = 1;
            statisticCount = 0;
            sum1 = 0; sum2 = 0; sum3 = 0; sum4 = 0;
        }

        private void Tab1_selected(object sender, TabControlEventArgs e)
        {

        }

        private void ViewerAlgorithem_select(object sender, EventArgs e)
        {
            simulationParamters.viewerAlgorithem = comboBox1.Text.ToString();
        }

        private void chart3_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            if (currenPCAsample !=simulationParamters.viewerNumOfSamples)
            {
                currenPCAsample += 1;
                PlotPCA(currenPCAsample);
            }
            Cursor = Cursors.Default;
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            if (currenPCAsample > 1)
            {
                currenPCAsample -= 1;
                PlotPCA(currenPCAsample);
            }
            Cursor = Cursors.Default;
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void IncludeUnknowBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            simulationParamters.includeUnknown = IncludeUnknowBox.Text.ToString();
        }

        private void robust_numOfCalls_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (robust_numOfCalls.Text.ToString().Length != 0)
                    simulationParamters.robustNumOfCalls = int.Parse(robust_numOfCalls.Text.ToString());
                else
                    simulationParamters.robustNumOfCalls = 0;
            }
            catch { robust_numOfCalls.Text = ""; }
        }

        private void robust_numOfSamples_SelectedIndexChanged(object sender, EventArgs e)
        {
            simulationParamters.robustNumOfSamples = int.Parse(robust_numOfSamples.Text.ToString());
        }

        private void robust_validationPercent_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (robust_validationPercent.Text.ToString().Length != 0)
                    simulationParamters.robustValidationPercent = double.Parse(robust_validationPercent.Text.ToString());
                else
                    simulationParamters.robustValidationPercent = 0;
            }
            catch { robust_validationPercent.Text = ""; }
        }

        private void run_Robust_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            WriteRobustConfigToFile(@"C:\Users\ramif\Desktop\Voip\RobustnessParams.csv");
            ExecutePythonCode("RobustnessApp.py");
            chartRobustness.Series.Clear();
            PlotRobustness();
            this.Cursor = Cursors.Default;
        }

        private void ViewerNumOfSamples_select(object sender, EventArgs e)
        {
            simulationParamters.viewerNumOfSamples = int.Parse(comboBox2.Text.ToString());
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void PCAincldeUnknown_SelectedIndexChanged(object sender, EventArgs e)
        {
            simulationParamters.pcaIncludeUnknown = PCAincldeUnknown.Text.ToString();
        }

        private void ViewerNumOfCalls_select(object sender, EventArgs e)
        {
            try
            {
                if (textBox3.Text.ToString().Length != 0)
                    simulationParamters.viewerNumOfCalls = int.Parse(textBox3.Text.ToString());
                else
                    simulationParamters.viewerNumOfCalls = 0;
            }
            catch { textBox3.Text = ""; }
        }

        private void ViewerRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            WritePCAConfigToFile(@"C:\Users\ramif\Desktop\Voip\PCAInput.csv");
            ExecutePythonCode("PCAapp.py");
            chart3.Series.Clear();
            currenPCAsample = 1;
            PlotPCA(currenPCAsample);
            this.Cursor = Cursors.Default;
        }
        private void PlotPCA(int sampleIndex)
        {
            chart3.Series.Clear();
            addSerise("Run 1", chart3, SeriesChartType.Point);

            chart3.Titles.Clear();
            chart3.Titles.Add("Title");
            chart3.Titles["Title1"].Font = new System.Drawing.Font("Maiandra GD", 16, FontStyle.Bold);
            chart3.Titles["Title1"].ForeColor = Color.White;
            chart3.ChartAreas[0].AxisX.Title = "PCA 1";
            chart3.ChartAreas[0].AxisX.TitleFont = new System.Drawing.Font("Maiandra GD", 22, FontStyle.Bold);
            chart3.ChartAreas[0].AxisX.TitleForeColor = Color.White;
            chart3.ChartAreas[0].AxisY.Title = "PCA 2";
            chart3.ChartAreas[0].AxisY.TitleFont = new System.Drawing.Font("Maiandra GD", 22, FontStyle.Bold);
            chart3.ChartAreas[0].AxisY.TitleForeColor = Color.White;
            chart3.ChartAreas[0].AxisX.LineColor= Color.White;
            chart3.ChartAreas[0].AxisX2.LineColor = Color.White;
            chart3.ChartAreas[0].AxisY.LineColor = Color.White;
            chart3.ChartAreas[0].AxisY2.LineColor = Color.White;
            chart3.ChartAreas[0].AxisY.LabelStyle.ForeColor= Color.White;
            chart3.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.White;
            chart3.ChartAreas[0].Axes[0].MajorGrid.LineColor = Color.White;
            chart3.ChartAreas[0].Axes[1].MajorGrid.LineColor = Color.White;
            chart3.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.#E-000}";
            chart3.ChartAreas[0].AxisY.LabelStyle.Format = "{0:0.#E-000}";
            int numberOfCalls = 0;
            try
            {
                using (StreamReader sr = new StreamReader(@"C:\Users\ramif\Desktop\Voip\RawData\PCA_results\" + sampleIndex.ToString() + "_samples_pca_output.csv"))
                {
                    string currentLine;
                    bool flag = true;
                    // currentLine will be null when the StreamReader reaches the end of file
                    while ((currentLine = sr.ReadLine()) != null)
                    {
                        if (!flag)
                        {
                            numberOfCalls++;
                            List<string> values = currentLine.Split(',').ToList<string>();
                            if (this.chart3.InvokeRequired)
                            {
                                this.BeginInvoke((MethodInvoker)(() =>
                                {
                                    this.chart3.Series["Run 1"].Points.AddXY(Math.Round(double.Parse(values[1]), 5) * 10000, Math.Round(double.Parse(values[2]), 5) * 10000);
                                    int index = chart3.Series["Run 1"].Points.Count - 1;
                                    //chart3.Series["Run 1"].Points[index].Color = GetColor(values[3]);
                                     chart3.Series["Run 1"].Points[index].MarkerStyle = MarkerStyle.Triangle;
                                    chart3.Series["Run 1"].Points[index].MarkerSize = 15;
                                    chart3.Series["Run 1"].Points[index].MarkerColor = GetColor(values[3]);
                                    chart3.Series["Run 1"].Points[0].LegendText = "wo";
                                }));
                            }
                            else
                            {
                                this.chart3.Series["Run 1"].Points.AddXY(Math.Round(double.Parse(values[1]), 5) * 10000, Math.Round(double.Parse(values[2]), 5) * 10000);
                                int index = chart3.Series["Run 1"].Points.Count - 1;
                                //chart3.Series["Run 1"].Points[index].Color = GetColor(values[3]);
                                chart3.Series["Run 1"].Points[index].MarkerStyle = MarkerStyle.Triangle;
                                chart3.Series["Run 1"].Points[index].MarkerSize = 15;
                                chart3.Series["Run 1"].Points[index].MarkerColor = GetColor(values[3]);
                                chart3.Series["Run 1"].Points[0].LegendText = "wo";


                            }

                        }
                        flag = false;
                    }
                }
            }
            catch { }
            chart3.Titles["Title1"].Text = "Sample Number: " + currenPCAsample.ToString()+ ", Number Of Calls: "+ numberOfCalls.ToString();
        }

        private void PlotRobustness()
        {
            chartRobustness.Series.Clear();
            addSerise("Run 1", chartRobustness, SeriesChartType.Column);

            chartRobustness.Titles.Clear();
            chartRobustness.Titles.Add("Random Forest Prdeiction");
            chartRobustness.Titles["Title1"].Font = new System.Drawing.Font("Maiandra GD", 16, FontStyle.Bold);
            chartRobustness.Titles["Title1"].ForeColor = Color.White;
            chartRobustness.ChartAreas[0].AxisX.Title = "Error Probability";
            chartRobustness.ChartAreas[0].AxisX.TitleFont = new System.Drawing.Font("Maiandra GD", 22, FontStyle.Bold);
            chartRobustness.ChartAreas[0].AxisX.TitleForeColor = Color.White;
            chartRobustness.ChartAreas[0].AxisY.Title = "Success Prediction Percent";
            chartRobustness.ChartAreas[0].AxisY.TitleFont = new System.Drawing.Font("Maiandra GD", 22, FontStyle.Bold);
            chartRobustness.ChartAreas[0].AxisY.TitleForeColor = Color.White;
            chartRobustness.ChartAreas[0].AxisX.LineColor = Color.White;
            chartRobustness.ChartAreas[0].AxisX2.LineColor = Color.White;
            chartRobustness.ChartAreas[0].AxisY.LineColor = Color.White;
            chartRobustness.ChartAreas[0].AxisY2.LineColor = Color.White;
            chartRobustness.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.White;
            chartRobustness.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.White;
            chartRobustness.ChartAreas[0].Axes[0].MajorGrid.LineColor = Color.White;
            chartRobustness.ChartAreas[0].Axes[1].MajorGrid.LineColor = Color.White;
            try
            {
                using (StreamReader sr = new StreamReader(@"C:\Users\ramif\Desktop\Voip\exports\resultsFor3Slide.csv"))
                {
                    string currentLine;
                    bool flag = true;
                    // currentLine will be null when the StreamReader reaches the end of file
                    while ((currentLine = sr.ReadLine()) != null)
                    {
                        if (!flag)
                        {
                            List<string> values = currentLine.Split(',').ToList<string>();
                            double v1 = double.Parse(values[0]);
                            double v2 = double.Parse(values[1]);
                            if (this.chartRobustness.InvokeRequired)
                            {
                                this.BeginInvoke((MethodInvoker)(() =>
                                {
                                    this.chartRobustness.Series["Run 1"].Points.AddXY(v1,v2);
                                    chartRobustness.Series["Run 1"].Points[chartRobustness.Series["Run 1"].Points.Count - 1].Label = v2.ToString();
                                    //chartRobustness.Series["Run 1"].Points[chartRobustness.Series["Run 1"].Points.Count - 1].LabelBackColor = Color.White;
                                    chartRobustness.Series["Run 1"].Points[chartRobustness.Series["Run 1"].Points.Count - 1].LabelForeColor = Color.White;
                                }));
                            }
                            else
                            {
                                this.chartRobustness.Series["Run 1"].Points.AddXY(v1, v2);
                                chartRobustness.Series["Run 1"].Points[chartRobustness.Series["Run 1"].Points.Count-1].Label = v2.ToString();
                                //chartRobustness.Series["Run 1"].Points[chartRobustness.Series["Run 1"].Points.Count - 1].LabelBackColor = Color.White;
                                chartRobustness.Series["Run 1"].Points[chartRobustness.Series["Run 1"].Points.Count - 1].LabelForeColor = Color.White;
                            }

                        }
                        flag = false;
                    }
                    
                }
            }
            catch { Exception ex; }
            //chartRobustness.Titles["Title1"].Text = "Sample Number: " + currenPCAsample.ToString() + ", Number Of Calls: " + numberOfCalls.ToString();
        }

    }
}

