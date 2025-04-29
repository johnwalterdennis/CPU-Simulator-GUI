using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CpuSchedulingWinForms
{
    public struct Metrics
    {
        public double AWT;
        public double ATT;
        public double Util;
        public double Throughput;
    }

    public static class Algorithms
    {
        //--------------------------------------------------------------------
        //  Generic helpers
        //--------------------------------------------------------------------
        private static int PromptInt(string caption, string title)
        {
            while (true)
            {
                string raw = Microsoft.VisualBasic.Interaction.InputBox(caption, title, "", -1, -1).Trim();
                if (int.TryParse(raw, out int v) && v >= 0) return v;
                MessageBox.Show("Please enter a valid non‑negative integer.", "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private static double PromptDouble(string caption, string title)
        {
            while (true)
            {
                string raw = Microsoft.VisualBasic.Interaction.InputBox(caption, title, "", -1, -1).Trim();
                if (double.TryParse(raw, out double v) && v >= 0) return v;
                MessageBox.Show("Please enter a valid non‑negative number.", "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static Metrics ComputeMetrics(double[] arrival, double[] burst, double[] waiting, double scheduleEnd)
        {
            int n = burst.Length;
            double busy = burst.Sum();
            double span = scheduleEnd - arrival.Min();
            if (span <= 0) span = 1;    // guard
            Metrics m;
            m.AWT = waiting.Average();
            m.ATT = waiting.Zip(burst, (w, b) => w + b).Average();
            m.Util = (busy / span) * 100.0;
            m.Throughput = n / span;
            return m;
        }

        private static void ShowMetrics(Metrics m, string caption)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Average Waiting Time   : {m.AWT:F2} sec");
            sb.AppendLine($"Average Turnaround Time: {m.ATT:F2} sec");
            sb.AppendLine($"CPU Utilisation        : {m.Util:F1} %");
            sb.AppendLine($"Throughput             : {m.Throughput:F3} proc/sec");
            MessageBox.Show(sb.ToString(), caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        //--------------------------------------------------------------------
        //  FCFS
        //--------------------------------------------------------------------
        public static void fcfsAlgorithm(string userInput)
        {
            if (!int.TryParse(userInput.Trim(), out int n) || n <= 0)
            {
                MessageBox.Show("Invalid number of processes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            double[] burst = new double[n];
            double[] waiting = new double[n];
            double[] arrival = new double[n];
            for (int i = 0; i < n; i++) arrival[i] = 0; // FCFS example assumes simultaneous arrival

            DialogResult res = MessageBox.Show("First‑Come‑First‑Serve Scheduling", "", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (res != DialogResult.Yes) return;

            for (int i = 0; i < n; i++)
                burst[i] = PromptDouble("Enter Burst time:", $"Burst time for P{i + 1}");

            waiting[0] = 0;
            for (int i = 1; i < n; i++)
            {
                waiting[i] = waiting[i - 1] + burst[i - 1];
                MessageBox.Show($"Waiting time for P{i + 1} = {waiting[i]}", "Waiting time", MessageBoxButtons.OK);
            }

            double scheduleEnd = burst.Sum();
            Metrics m = ComputeMetrics(arrival, burst, waiting, scheduleEnd);
            ShowMetrics(m, "FCFS Metrics");
        }

        //--------------------------------------------------------------------
        //  SJF (non‑pre‑emptive)
        //--------------------------------------------------------------------
        public static void sjfAlgorithm(string userInput)
        {
            if (!int.TryParse(userInput.Trim(), out int n) || n <= 0)
            {
                MessageBox.Show("Invalid number of processes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            double[] burst = new double[n];
            double[] waiting = new double[n];
            double[] arrival = new double[n];
            for (int i = 0; i < n; i++) arrival[i] = 0;

            DialogResult res = MessageBox.Show("Shortest Job First Scheduling", "", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (res != DialogResult.Yes) return;

            for (int i = 0; i < n; i++)
                burst[i] = PromptDouble("Enter Burst time:", $"Burst time for P{i + 1}");

            // sort by burst while keeping index map
            int[] idx = Enumerable.Range(0, n).ToArray();
            Array.Sort(idx, (a, b) => burst[a].CompareTo(burst[b]));

            double elapsed = 0;
            foreach (int p in idx)
            {
                waiting[p] = elapsed;
                elapsed += burst[p];
                MessageBox.Show($"Waiting time for P{p + 1} = {waiting[p]}", "Waiting time", MessageBoxButtons.OK);
            }
            Metrics m = ComputeMetrics(arrival, burst, waiting, elapsed);
            ShowMetrics(m, "SJF Metrics");
        }

        //--------------------------------------------------------------------
        //  Priority (non‑pre‑emptive, lower value → higher priority)
        //--------------------------------------------------------------------
        public static void priorityAlgorithm(string userInput)
        {
            if (!int.TryParse(userInput.Trim(), out int n) || n <= 0)
            {
                MessageBox.Show("Invalid number of processes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            double[] burst = new double[n];
            double[] waiting = new double[n];
            int[] priority = new int[n];
            double[] arrival = new double[n];
            for (int i = 0; i < n; i++) arrival[i] = 0;

            DialogResult res = MessageBox.Show("Priority Scheduling", "", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (res != DialogResult.Yes) return;

            for (int i = 0; i < n; i++)
            {
                burst[i] = PromptDouble("Enter Burst time:", $"Burst time for P{i + 1}");
                priority[i] = PromptInt("Enter Priority:", $"Priority for P{i + 1}");
            }

            int[] idx = Enumerable.Range(0, n).ToArray();
            Array.Sort(idx, (a, b) => priority[a].CompareTo(priority[b]));

            double elapsed = 0;
            foreach (int p in idx)
            {
                waiting[p] = elapsed;
                elapsed += burst[p];
                MessageBox.Show($"Waiting time for P{p + 1} = {waiting[p]}", "Waiting time", MessageBoxButtons.OK);
            }
            Metrics m = ComputeMetrics(arrival, burst, waiting, elapsed);
            ShowMetrics(m, "Priority Metrics");
        }

        //--------------------------------------------------------------------
        //  Round Robin (time‑quantum from Helper.QuantumTime)
        //--------------------------------------------------------------------
        public static void roundRobinAlgorithm(string userInput)
        {
            if (!int.TryParse(userInput.Trim(), out int n) || n <= 0)
            {
                MessageBox.Show("Invalid number of processes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            double[] arrival = new double[n];
            double[] burst = new double[n];
            double[] remaining = new double[n];
            double[] waiting = new double[n];

            DialogResult res = MessageBox.Show("Round Robin Scheduling", "", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (res != DialogResult.Yes) return;

            for (int i = 0; i < n; i++)
            {
                arrival[i] = PromptDouble("Enter Arrival time:", $"Arrival time for P{i + 1}");
                burst[i] = PromptDouble("Enter Burst time:", $"Burst time for P{i + 1}");
                remaining[i] = burst[i];
            }
            double quantum = PromptDouble("Enter Time Quantum:", "Time Quantum");
            Helper.QuantumTime = quantum.ToString();

            double time = arrival.Min();
            int complete = 0;
            while (complete < n)
            {
                bool didWork = false;
                for (int i = 0; i < n; i++)
                {
                    if (remaining[i] > 0 && arrival[i] <= time)
                    {
                        double slice = Math.Min(quantum, remaining[i]);
                        remaining[i] -= slice;
                        time += slice;
                        didWork = true;
                        if (remaining[i] == 0)
                        {
                            waiting[i] = time - arrival[i] - burst[i];
                            MessageBox.Show($"Waiting time for P{i + 1} = {waiting[i]}", "Waiting time", MessageBoxButtons.OK);
                            complete++;
                        }
                    }
                }
                if (!didWork) time++; // idle gap
            }
            Metrics m = ComputeMetrics(arrival, burst, waiting, time);
            ShowMetrics(m, "Round Robin Metrics");
        }

        //--------------------------------------------------------------------
        //  SRTF (pre‑emptive SJF)
        //--------------------------------------------------------------------
        public static void srtfAlgorithm(string userInput)
        {
            if (!int.TryParse(userInput.Trim(), out int n) || n <= 0)
            {
                MessageBox.Show("Invalid number of processes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            double[] arrival = new double[n];
            double[] burst = new double[n];
            double[] remaining = new double[n];
            double[] waiting = new double[n];

            DialogResult res = MessageBox.Show("Shortest Remaining Time First (Pre‑emptive)", "", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (res != DialogResult.Yes) return;

            for (int i = 0; i < n; i++)
            {
                arrival[i] = PromptDouble("Enter Arrival time:", $"Arrival time for P{i + 1}");
                burst[i] = PromptDouble("Enter Burst time:", $"Burst time for P{i + 1}");
                remaining[i] = burst[i];
            }

            double time = arrival.Min();
            int finished = 0;
            while (finished < n)
            {
                int idx = -1;
                double minRem = double.MaxValue;
                for (int i = 0; i < n; i++)
                    if (arrival[i] <= time && remaining[i] > 0 && remaining[i] < minRem)
                    { idx = i; minRem = remaining[i]; }

                if (idx == -1) { time++; continue; }

                remaining[idx] -= 1;  // one‑unit quantum
                time += 1;
                if (remaining[idx] == 0)
                {
                    waiting[idx] = time - arrival[idx] - burst[idx];
                    MessageBox.Show($"Waiting time for P{idx + 1} = {waiting[idx]}", "Waiting time", MessageBoxButtons.OK);
                    finished++;
                }
            }
            Metrics m = ComputeMetrics(arrival, burst, waiting, time);
            ShowMetrics(m, "SRTF Metrics");
        }

        //--------------------------------------------------------------------
        //  HRRN (Highest Response‑Ratio Next)
        //--------------------------------------------------------------------
        public static void hrrnAlgorithm(string userInput)
        {
            if (!int.TryParse(userInput.Trim(), out int n) || n <= 0)
            {
                MessageBox.Show("Invalid number of processes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            double[] arrival = new double[n];
            double[] burst = new double[n];
            double[] waiting = new double[n];
            bool[] done = new bool[n];

            DialogResult res = MessageBox.Show("Highest Response‑Ratio Next", "", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (res != DialogResult.Yes) return;

            for (int i = 0; i < n; i++)
            {
                arrival[i] = PromptDouble("Enter Arrival time:", $"Arrival time for P{i + 1}");
                burst[i] = PromptDouble("Enter Burst time:", $"Burst time for P{i + 1}");
            }

            double time = arrival.Min();
            int completed = 0;
            while (completed < n)
            {
                List<int> ready = new List<int>();
                for (int i = 0; i < n; i++)
                    if (!done[i] && arrival[i] <= time) ready.Add(i);

                if (ready.Count == 0)
                {
                    time = arrival.Where((a, idx) => !done[idx]).Min();
                    continue;
                }

                double bestRatio = -1; int chosen = ready[0];
                foreach (int i in ready)
                {
                    double ratio = (time - arrival[i] + burst[i]) / burst[i];
                    if (ratio > bestRatio) { bestRatio = ratio; chosen = i; }
                }

                waiting[chosen] = time - arrival[chosen];
                MessageBox.Show($"Waiting time for P{chosen + 1} = {waiting[chosen]}", "Waiting time", MessageBoxButtons.OK);
                time += burst[chosen];
                done[chosen] = true;
                completed++;
            }
            Metrics m = ComputeMetrics(arrival, burst, waiting, time);
            ShowMetrics(m, "HRRN Metrics");
        }
    }
}
