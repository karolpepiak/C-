using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Xml.Linq;

namespace PloterGUI
{
    public partial class Form1 : Form
    {
        public static Form1 instance;

        SerialPort pump_port;
        SerialPort ploter_port;

        //bools
        bool ploter_connection = false;
        bool pump_connection = false;
        bool temp_bool = false;
        bool ploter_port_connection = false;
        bool pump_port_connection = false;
        bool set_speed = false;
        bool set_start_pos = false;
        bool set_pumping_rate = false;
        bool set_pumping_rate_units = false;
        bool set_syringe_inside_diameter = false;
        bool x_move = false;
        bool y_move = false;

        //line counter
        public int line_counter = 0;

        //intigers responsible for all possitioning operations
        int x_pos = 0;
        int y_pos = 0;
        int pos_of_new_line = 0;
        int pos_of_dot = 0;
        int pos_of_F_in_line = 0;
        int pos_of_X_in_line = 0;
        int pos_of_Y_in_line = 0;
        int end_pos_of_F_value_in_line = 0;
        int end_pos_of_X_value_in_line = 0;
        int end_pos_of_Y_value_in_line = 0;
        int Home_X_pos = 0;
        int Home_Y_pos = 0;
        int X_move_value = 0;
        int Y_move_value = 0;

        //user input intigers
        int num_of_lines = 0;
        int repeat = 1;
        int template = 0;
        int speed = 0;
        int lenght_of_text = 0;

        //temporary buffors
        int intiger_buffor_1 = 0;
        string string_buffor_1 = String.Empty;
        string string_buffor_2 = String.Empty;
        string string_buffor_3 = String.Empty;

        //doubles
        double default_pumping_rate = 2.5;
        double pumping_rate = 0;
        double default_syringe_inside_diameter = 22.5;
        double syringe_inside_diameter = 0;
        double pump_work_time_in_sec = 0;

        //strings
        string message = string.Empty;
        string pumping_rate_units = String.Empty;
        string pumping_rate_units_to_send = String.Empty;
        string pump_serial_port = String.Empty;
        string ploter_serial_port = String.Empty;
        string path_template = string.Empty;

        //constant strings
        string default_pumping_rate_units = "mL/hr";
        string default_pumping_rate_units_to_send = "MH";
        string path_to_save = Path.Combine(Environment.CurrentDirectory, "Logs.txt");
        string serial_port_name = "COM";
        string path_importer = "Importer.txt";
        string template_1 = "template_1.txt";
        string template_2 = "template_2.txt";
        string template_3 = "template_3.txt";
        string template_4 = "template_4.txt";

        //tables
        double[] move_value = new double[1000];
        string[] line = new string[1000];

        //check if input port names are correct
        public void check_port_name(string textbox_info)
        {
            //check string length
            if (textbox_info == null || textbox_info.Length < 3)
            {
                //display logs
                this.textBox3.Text = this.textBox3.Text + line_counter + ": Wrong data format (must be COM# in range 1-100)" + "\r\n";
                line_counter++;
            }
            else
            {
                //check name
                string_buffor_2 = textbox_info.Substring(0, 3);
                intiger_buffor_1 = int.Parse(textbox_info.Substring(3, textbox_info.Length - 3));
                //sprawdza czy pierwsze 3 litery nazwy portu pompy = "com" z pominieciem wielkosci liter i sprawdza czy numer portu miesci sie pomiedzy 1-100
                if (textbox_info.IndexOf(serial_port_name, StringComparison.OrdinalIgnoreCase) >= 0 && Enumerable.Range(1, 100).Contains(intiger_buffor_1))
                {
                    //display logs
                    this.textBox3.Text = this.textBox3.Text + line_counter + ": Pump Serial Port has been changed to: " + textbox_info.ToUpper() + "\r\n";
                    line_counter++;
                }
                else
                {
                    //display logs
                    this.textBox3.Text = this.textBox3.Text + line_counter + ": Wrong data format for pump port name (must be COM# in range 1-100)" + "\r\n";
                    line_counter++;
                }
                string_buffor_1 = string.Empty;
                string_buffor_2 = string.Empty;
                string_buffor_3 = string.Empty;
            }
        }

        //create serial connection with pump
        public void create_pump_connection()
        {
            pump_serial_port = this.textBox8.Text;
            check_port_name(pump_serial_port);
            try
            {
                //set settings of pump serial connection
                pump_port = new SerialPort();
                pump_port.PortName = pump_serial_port.ToUpper();
                pump_port.BaudRate = 9600;
                pump_port.DataBits = 8;
                pump_port.StopBits = StopBits.One;
                pump_port.Handshake = Handshake.None;
                pump_port.Parity = Parity.None;
                pump_port.ReadTimeout = 500;
                pump_port.WriteTimeout = 500;
            }
            catch
            {
                //display logs
                this.textBox3.Text = this.textBox3.Text + line_counter + ": Pump port name has not been set" + "\r\n";
                line_counter++;
            }
        }

         //create serial connection with plotter
        public void create_ploter_connection()
        {
            ploter_serial_port = this.textBox9.Text;
            check_port_name(ploter_serial_port);
            try
            {
                //set settings of plotter serial connection
                ploter_port = new SerialPort();
                ploter_port.PortName = ploter_serial_port.ToUpper();
                ploter_port.BaudRate = 115200;
                ploter_port.DataBits = 8;
                ploter_port.StopBits = StopBits.One;
                ploter_port.Parity = Parity.None;
                ploter_port.ReadTimeout = 500;
                ploter_port.WriteTimeout = 500;
            }
            catch
            {
                //display logs
                this.textBox3.Text = this.textBox3.Text + line_counter + ": Ploter port name has not been set" + "\r\n";
                line_counter++;
            }
        }
        
        //open pump connection
        public void open_pump_connection()
        {
            //tries to open pump connection
            try
            {
                pump_port.Open();
                //display logs
                this.textBox3.Text = this.textBox3.Text + line_counter + ": Port " + pump_serial_port.ToUpper() + " is visible" + "\r\n";
                line_counter++;
                //display logs
                this.textBox3.Text = this.textBox3.Text + line_counter + ": Pump connected" + "\r\n";
                line_counter++;
                pump_port_connection = true;
            }
            //if not able to open connection
            catch (Exception ex)
            {
                //display logs
                this.textBox3.Text = this.textBox3.Text + line_counter + ": Port " + pump_serial_port.ToUpper() + " is unvisible" + "\r\n";
                line_counter++;
                this.textBox3.Text = this.textBox3.Text + line_counter + ": Check cable connection" + "\r\n";
                line_counter++;
                this.textBox3.Text = this.textBox3.Text + line_counter + ": Check if Pump Baude Rate is set to 9600 b/s" + "\r\n";
                line_counter++;
                pump_port_connection = false;
            }
        }

        //open plotter connection
        public void open_ploter_connection()
        {
            //tries to open pump connection
            try
            {
                ploter_port.Open();
                //display logs
                this.textBox3.Text = this.textBox3.Text + line_counter + ": Port " + ploter_serial_port.ToUpper() + " is visible" + "\r\n";
                line_counter++;
                //display logs
                this.textBox3.Text = this.textBox3.Text + line_counter + ": Ploter connected" + "\r\n";
                line_counter++;
                ploter_port_connection = true;
            }
            //if not able to open connection
            catch (Exception ex)
            {
                //display logs
                this.textBox3.Text = this.textBox3.Text + line_counter + ": Port " + ploter_serial_port.ToUpper() + " is unvisible" + "\r\n";
                line_counter++;
                this.textBox3.Text = this.textBox3.Text + line_counter + ": Check cable connection" + "\r\n";
                line_counter++;
                ploter_port_connection = false;
            }
        }

        //calculate movement patch of plotter head
        public void count_path(string  line, int line_index)
        {
            X_move_value = 0;
            Y_move_value = 0;
            //X and Y direction movement
            if (x_move == true && y_move == true)
            {
                pos_of_X_in_line = line.IndexOf("X");
                string_buffor_1 = line.Substring(pos_of_X_in_line, line.Length - pos_of_X_in_line);
                end_pos_of_X_value_in_line = string_buffor_1.IndexOf(" ");
                string_buffor_2 = line.Substring(pos_of_X_in_line, end_pos_of_X_value_in_line);

                //movement in direction -X
                if (string_buffor_1.IndexOf("-", 0, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    string_buffor_3 = string_buffor_1.Substring(2, string_buffor_2.Length - 2);
                }
                //movement in direction +X
                else
                {
                    string_buffor_3 = string_buffor_1.Substring(1, string_buffor_2.Length - 1);
                }

                X_move_value = int.Parse(string_buffor_3);
                x_pos = x_pos + X_move_value;

                string_buffor_1 = String.Empty;
                string_buffor_2 = String.Empty;
                string_buffor_3 = String.Empty;

                pos_of_Y_in_line = line.IndexOf("Y");
                string_buffor_1 = line.Substring(pos_of_Y_in_line, line.Length - pos_of_Y_in_line);
                end_pos_of_Y_value_in_line = string_buffor_1.IndexOf("\r\n");
                string_buffor_2 = line.Substring(pos_of_Y_in_line, end_pos_of_Y_value_in_line);

                //movement in direction -Y
                if (string_buffor_1.IndexOf("-", 0, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    string_buffor_3 = string_buffor_1.Substring(2, string_buffor_2.Length - 2);
                }
                //movement in direction +Y
                else
                {
                    string_buffor_3 = string_buffor_1.Substring(1, string_buffor_2.Length - 1);
                }

                Y_move_value = int.Parse(string_buffor_3);
                y_pos = y_pos + Y_move_value;

                string_buffor_1 = String.Empty;
                string_buffor_2 = String.Empty;
                string_buffor_3 = String.Empty;

                //calculating path
                move_value[line_index] = Math.Sqrt((Y_move_value * Y_move_value) + (X_move_value * X_move_value)); 
            }
            //only X direction movement
            else if (x_move == true && y_move == false)
            {
                pos_of_X_in_line = line.IndexOf("X");
                string_buffor_1 = line.Substring(pos_of_X_in_line, line.Length - pos_of_X_in_line);
                end_pos_of_X_value_in_line = string_buffor_1.IndexOf("\r\n");

                //movement in direction -X
                if (string_buffor_1.IndexOf("-", 0, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    string_buffor_2 = string_buffor_1.Substring(2, string_buffor_1.Length - end_pos_of_X_value_in_line);
                }
                //movement in direction +X
                else
                {
                    string_buffor_2 = string_buffor_1.Substring(1, string_buffor_1.Length - end_pos_of_X_value_in_line);
                }

                X_move_value = int.Parse(string_buffor_2);
                x_pos = x_pos + X_move_value;

                string_buffor_1 = String.Empty;
                string_buffor_2 = String.Empty;

                //calculating path
                move_value[line_index] = X_move_value;
            }
            //only Y direction movement
            else if (x_move == false && y_move == true)
            {
                pos_of_Y_in_line = line.IndexOf("Y");
                string_buffor_1 = line.Substring(pos_of_Y_in_line, line.Length - pos_of_Y_in_line);
                end_pos_of_Y_value_in_line = string_buffor_1.IndexOf("\r\n");

                //movement in direction -Y
                if (string_buffor_1.IndexOf("-", 0, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    string_buffor_2 = string_buffor_1.Substring(2, string_buffor_1.Length - end_pos_of_Y_value_in_line);
                }
                //movement in direction +Y
                else
                {
                    string_buffor_2 = string_buffor_1.Substring(1, string_buffor_1.Length - end_pos_of_Y_value_in_line);
                }

                Y_move_value = int.Parse(string_buffor_2);
                y_pos = y_pos + Y_move_value;

                string_buffor_1 = String.Empty;
                string_buffor_2 = String.Empty;

                //calculating path
                move_value[line_index] = Y_move_value;
            }
        }

        //import .txt file
        public void import_file(string file_name)
        {
            try
            {
                //clear textbox
                this.textBox1.Clear();
                //get path name
                path_template = Path.Combine(Environment.CurrentDirectory, file_name);
                //hide template panel
                hide_submenu();
                using (StreamReader sr = File.OpenText(path_template))
                {
                    while ((string_buffor_1 = sr.ReadLine()) != null)
                    {
                        this.textBox1.Text = this.textBox1.Text + string_buffor_1 + "\r\n";
                    }
                }
                string_buffor_1 = string.Empty;
                //display logs
                this.textBox3.Text = this.textBox3.Text + line_counter + ": " + file_name + " import successful" + "\r\n";
                line_counter++;
            }
            catch
            {
                //display logs
                this.textBox3.Text = this.textBox3.Text + line_counter + ": " + file_name + " import unsuccessful" + "\r\n";
                line_counter++;
            }
        }

        //initializes Form1
        public Form1()
        {
            InitializeComponent();
            panel_visible();
            instance = this;
        }

        //changes visibility of template panel
        private void panel_visible()
        {
            panel1.Visible = false;
        }

        //hide template panel
        private void hide_submenu()
        {
            if(panel1.Visible == true)
            {
                panel1.Visible = false;
            }
        }

        //show template panel
        private void show_submenu()
        {
            if(panel1.Visible == false)
            {
                panel1.Visible = true;
            }
        }

        //Pump parametres - label
        private void label1_Click(object sender, EventArgs e)
        {

        }

        //Set pumping rate - label
        private void label2_Click(object sender, EventArgs e)
        {

        }

        //Set pumping rate - button
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (pump_port_connection == false)
                {
                    //tries to create pump connection
                    create_pump_connection();
                    //tries to open pump connection
                    open_pump_connection();
                }

                //set bool if textBox4 have any text
                temp_bool = String.IsNullOrEmpty(this.textBox4.Text);

                //if empty
                if (temp_bool == true)
                {
                    set_pumping_rate = false;
                }
                else
                {
                    string_buffor_1 = this.textBox4.Text;
                    try
                    {
                        set_pumping_rate = true;
                        pumping_rate = double.Parse(string_buffor_1);
                        pumping_rate = Math.Round(pumping_rate, 2);
                    }
                    catch
                    {
                        set_pumping_rate = false;
                        //display logs
                        this.textBox3.Text = this.textBox3.Text + line_counter + ": Could not parse: " + pumping_rate.ToString() + " to double format" + "\r\n";
                        line_counter++;
                    }
                }

                //if units checked
                if (this.checkBox1.Checked || this.checkBox2.Checked || this.checkBox3.Checked || this.checkBox4.Checked)
                {
                    set_pumping_rate_units = true;

                    //If uL/min checked
                    if (this.checkBox1.Checked)
                    {
                        pumping_rate_units_to_send = "UM";
                    }
                    //If mL/min checked
                    else if (this.checkBox2.Checked)
                    {
                        pumping_rate_units_to_send = "MM";
                    }
                    //If uL/hr checked
                    else if (this.checkBox3.Checked)
                    {
                        pumping_rate_units_to_send = "UH";
                    }
                    //If mL/hr checked
                    else
                    {
                        pumping_rate_units_to_send = "MH";
                    }
                }
                else
                {
                    //default settings
                    set_pumping_rate_units = false;
                }

                //pumping rate not set
                if (set_pumping_rate == false)
                {
                    pumping_rate = default_pumping_rate;
                }

                //pumping rate units not set
                if (set_pumping_rate_units == false)
                {
                    pumping_rate_units_to_send = default_pumping_rate_units_to_send;
                }

                try
                {
                    if (pumping_rate.ToString().Contains(',') == true)
                    {
                        pos_of_dot = pumping_rate.ToString().IndexOf(",");
                        string_buffor_1 = pumping_rate.ToString().Substring(0, pos_of_dot);
                        string_buffor_2 = pumping_rate.ToString().Substring(pos_of_dot + 1, pumping_rate.ToString().Length - (pos_of_dot + 1));
                        string_buffor_3 = string_buffor_1 + "." + string_buffor_2;
                        
                        //Send data to pump
                        pump_port.WriteLine(String.Format("RAT I " + string_buffor_3 + " " + pumping_rate_units_to_send + "\r\n"));
                        //display logs
                        this.textBox3.Text = this.textBox3.Text + line_counter + ": Pumping rate has been changed to: " + string_buffor_3 + " " + pumping_rate_units + "\r\n";
                        line_counter++;
                    }
                    else
                    {
                        //Przeslanie danych na wejscie pompy
                        pump_port.WriteLine(String.Format("RAT I " + pumping_rate.ToString() + " " + pumping_rate_units_to_send + "\r\n"));
                        //wyswietlanie logow
                        this.textBox3.Text = this.textBox3.Text + line_counter + ": Pumping rate has been changed to: " + pumping_rate.ToString() + " " + pumping_rate_units + "\r\n";
                        line_counter++;
                    }
                }
                catch
                {
                    //wyswietlanie logow
                    this.textBox3.Text = this.textBox3.Text + line_counter + ": Pumping rate has not been changed" + "\r\n";
                    line_counter++;
                }

                //czyszczenie bufforow
                string_buffor_1 = string.Empty;
                string_buffor_2 = string.Empty;
                string_buffor_3 = string.Empty;
            }
            catch
            {
                //wyswietlanie logow
                this.textBox3.Text = this.textBox3.Text + line_counter + ": Pump is disconnected " + "\r\n";
                line_counter++;
            }
        }

        //Ustaw szybkosc pompowania - TextBox
        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        //Szybkosc pompowania - CheckBox uL/min
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox1.Checked)
            {
                pumping_rate_units = "uL/min";
                this.checkBox2.Checked = false;
                this.checkBox3.Checked = false;
                this.checkBox4.Checked = false;
            }
        }

        //Szybkosc pompowania - CheckBox mL/min
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox2.Checked)
            {
                pumping_rate_units = "mL/min";
                this.checkBox1.Checked = false;
                this.checkBox3.Checked = false;
                this.checkBox4.Checked = false;
            }
        }

        //Szybkosc pompowania - CheckBox uL/hr
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox3.Checked)
            {
                pumping_rate_units = "uL/hr";
                this.checkBox1.Checked = false;
                this.checkBox2.Checked = false;
                this.checkBox4.Checked = false;
            }
        }

        //Szybkosc pompowania - CheckBox mL/hr
        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox4.Checked)
            {
                pumping_rate_units = "mL/hr";
                this.checkBox1.Checked = false;
                this.checkBox2.Checked = false;
                this.checkBox3.Checked = false;
            }
        }
        
        //Srednica wewnatrz strzykawki [mm] - Label
        private void label3_Click(object sender, EventArgs e)
        {

        }

        //Srednica wewnatrz strzykawki [mm] - Button
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (pump_port_connection == false)
                {
                    //proba utworzenia danych portu
                    create_pump_connection();
                    //proba otwarcia portu
                    open_pump_connection();
                }

                //bool z informacja czy textBox5 zawiera jakies dane
                temp_bool = String.IsNullOrEmpty(this.textBox5.Text);

                //jesli jest pusty
                if (temp_bool == true)
                {
                    set_syringe_inside_diameter = false;
                }
                //jesli zawiera
                else
                {
                    string_buffor_1 = this.textBox5.Text;
                    try
                    {
                        set_syringe_inside_diameter = true;
                        //parse z string do double
                        syringe_inside_diameter = double.Parse(string_buffor_1);
                        //zaokraglenie do dwuch miejsc po przecinku
                        syringe_inside_diameter = Math.Round(syringe_inside_diameter, 2);
                    }
                    catch
                    {
                        set_syringe_inside_diameter = false;
                        //wyswietlanie logow
                        this.textBox3.Text = this.textBox3.Text + line_counter + ": Could not parse: " + syringe_inside_diameter.ToString() + " to double format" + "\r\n";
                        line_counter++;
                    }
                }

                //jezeli nie ustawiono srednicy wewnetrznej strzykawki
                if (set_syringe_inside_diameter == false)
                {
                    syringe_inside_diameter = default_syringe_inside_diameter;
                }

                try
                {
                    if (syringe_inside_diameter.ToString().Contains(',') == true)
                    {
                        pos_of_dot = syringe_inside_diameter.ToString().IndexOf(",");
                        string_buffor_1 = syringe_inside_diameter.ToString().Substring(0, pos_of_dot);
                        string_buffor_2 = syringe_inside_diameter.ToString().Substring(pos_of_dot + 1, syringe_inside_diameter.ToString().Length - (pos_of_dot + 1));
                        string_buffor_3 = string_buffor_1 + "." + string_buffor_2;
                        //Przeslanie danych na wejscie pompy
                        pump_port.WriteLine(String.Format("DIA " + string_buffor_3 + "\r\n"));
                        //wyswietlanie logow
                        this.textBox3.Text = this.textBox3.Text + line_counter + ": Syringe inside diameter has been chaged to: " + syringe_inside_diameter.ToString() + "mm" + "\r\n";
                        line_counter++;
                    }
                    else
                    {
                        //Przeslanie danych na wejscie pompy
                        pump_port.WriteLine(String.Format("DIA " + syringe_inside_diameter.ToString() + "\r\n"));
                        //wyswietlanie logow
                        this.textBox3.Text = this.textBox3.Text + line_counter + ": Syringe inside diameter has been chaged to: " + syringe_inside_diameter.ToString() + "mm" + "\r\n";
                        line_counter++;
                    }
                }
                catch
                {
                    //wyswietlanie logow
                    this.textBox3.Text = this.textBox3.Text + line_counter + ": Syringe inside diameter has not been changed" + "\r\n";
                    line_counter++;
                }

                //czyszczenie bufforow
                string_buffor_1 = string.Empty;
                string_buffor_2 = string.Empty;
                string_buffor_3 = string.Empty;
            }
            catch
            {
                //wyswietlanie logow
                this.textBox3.Text = this.textBox3.Text + line_counter + ": Pump is disconnected " + "\r\n";
                line_counter++;
            }
        }

        //Srednica wewnatrz strzykawki [mm] - TextBox
        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        //Parametry plotera - Label
        private void label4_Click(object sender, EventArgs e)
        {

        }

        //Predkosc ruchu glowicy plotera [mm/s] - Label
        private void label5_Click(object sender, EventArgs e)
        {

        }

        //Predkosc ruchu glowicy plotera [mm/s] - Button
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                set_speed = true;
                //parse z string do int
                speed = int.Parse(this.textBox6.Text);
                //wyswietlanie logow
                this.textBox3.Text = this.textBox3.Text + line_counter + ": Speed has been changed to: " + speed.ToString() + " mm/min" + "\r\n";
                line_counter++;
            }
            catch
            {
                set_speed = false;
                //wyswietlanie logow
                this.textBox3.Text = this.textBox3.Text + line_counter + ": Speed input wrong format" + "\r\n";
                line_counter++;
            } 
        }

        //Predkosc ruchu glowicy plotera [mm/s] - TextBox
        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        //Ustaw pozycje startowa (HOME) - Label
        private void label6_Click(object sender, EventArgs e)
        {

        }

        //Ustaw pozycje startowa (HOME) - Button
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                Home_X_pos = int.Parse(this.textBox7.Text);
                Home_Y_pos = int.Parse(this.textBox10.Text);
                //wyswietlanie logow
                this.textBox3.Text = this.textBox3.Text + line_counter + ": Start Position has been changed to X:" + Home_X_pos.ToString() + " Y:" + Home_Y_pos.ToString() + "\r\n";
                line_counter++;
            }
            catch
            {
                try
                {
                    Home_X_pos = int.Parse(this.textBox7.Text);
                    //wyswietlanie logow
                    this.textBox3.Text = this.textBox3.Text + line_counter + ": Start Position Y value input wrong format" + "\r\n";
                    line_counter++;
                }
                catch
                {
                    //wyswietlanie logow
                    this.textBox3.Text = this.textBox3.Text + line_counter + ": Start Position X value input wrong format" + "\r\n";
                    line_counter++;
                }
            }
        }

        //Ustaw pozycje startowa (X) - X Label
        private void label12_Click(object sender, EventArgs e)
        {

        }

        //Ustaw pozycje startowa (X) - TextBox
        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }

        //Ustaw pozycje startowa (Y) - Y Label
        private void label13_Click(object sender, EventArgs e)
        {

        }

        //Ustaw pozycje startowa (Y) - Y TextBox
        private void textBox10_TextChanged(object sender, EventArgs e)
        {

        }

        //Idz do pozycji startowej - Label
        private void label7_Click(object sender, EventArgs e)
        {

        }

        //Idz do pozycji startowej - Button
        private void button5_Click(object sender, EventArgs e)
        {
            //tworzy polaczenia serial COM
            create_ploter_connection();
            //probuje nawiazac polaczenie z ploterem i pompa
            open_ploter_connection();

            //jezeli nie ustawiono pozycji startowej
            if (set_start_pos == false)
            {
                Home_X_pos = 0;
                Home_Y_pos = 0;
            }

            try
            {
                //idzie do kata
                ploter_port.WriteLine(String.Format("F1000 G91 G00 X-" + (x_pos + 200).ToString() + " Y-" + (y_pos + 127).ToString()));
                this.textBox3.Text = this.textBox3.Text + line_counter + ": Going to corner" + "\r\n";
                line_counter++;
                //idzie do pozycji Home
                ploter_port.WriteLine(String.Format("F1000 G91 G00 X" + (Home_X_pos + 200).ToString() + " Y" + (Home_Y_pos + 127).ToString()));
                this.textBox3.Text = this.textBox3.Text + line_counter + ": Going to Home position" + "\r\n";
                line_counter++;

                x_pos = Home_X_pos;
                y_pos = Home_Y_pos;
            }
            catch
            {
                //wyswietlanie logow
                this.textBox3.Text = this.textBox3.Text + line_counter + ": Operation Failed" + "\r\n";
                line_counter++;
            }

            //zamyka polaczenia
            ploter_port.Close();
        }

        //Logi - Label
        private void label8_Click(object sender, EventArgs e)
        {

        }

        //Logi - TextBox
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            textBox3.ScrollBars = ScrollBars.Vertical;
        }

        //Zapisz logi do pliku - Button
        private void button12_Click(object sender, EventArgs e)
        {
            TextWriter tw = new StreamWriter(path_to_save);
            tw.WriteLine(textBox3.Text);
            tw.Flush();
            tw.Close();
        }

        //Port serial pompa - Label
        private void label10_Click(object sender, EventArgs e)
        {

        }

        //Ploter serial pompa - Label
        private void label11_Click(object sender, EventArgs e)
        {

        }

        //laczy sie z pompa i ploterem - Button
        private void button15_Click(object sender, EventArgs e)
        {
            try
            {
                if (pump_port_connection == false)
                {
                    try
                    {
                        //proba utworzenia danych portu
                        create_pump_connection();
                        //proba otwarcia portu
                        open_pump_connection();
                    }
                    catch
                    {
                        //wyswietlanie logow
                        this.textBox3.Text = this.textBox3.Text + line_counter + ": Could not connect to pump" + "\r\n";
                        line_counter++;
                    }
                }
                else
                {
                    //wyswietlanie logow
                    this.textBox3.Text = this.textBox3.Text + line_counter + ": Pump connected" + "\r\n";
                    line_counter++;
                }
                if (ploter_port_connection == false)
                {
                    try
                    {
                        //proba utworzenia danych portu
                        create_ploter_connection();
                        //proba otwarcia portu
                        open_ploter_connection();
                    }
                    catch
                    {
                        //wyswietlanie logow
                        this.textBox3.Text = this.textBox3.Text + line_counter + ": Could not connect to ploter" + "\r\n";
                        line_counter++;
                    }
                }
                else
                {
                    //wyswietlanie logow
                    this.textBox3.Text = this.textBox3.Text + line_counter + ": Ploter connected" + "\r\n";
                    line_counter++;
                }
            }
            catch
            {
                //wyswietlanie logow
                this.textBox3.Text = this.textBox3.Text + line_counter + ": Could not connect to ports" + "\r\n";
                line_counter++;
            }
        }

        //Port serial pompa - TextBox
        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }

        //Ploter serial pompa - TextBox
        private void textBox9_TextChanged(object sender, EventArgs e)
        {

        }

        //Sprawdz polaczenie - Button
        private void button13_Click(object sender, EventArgs e)
        {
            {
                try
                {
                    //wyswietlanie logow
                    this.textBox3.Text = this.textBox3.Text + line_counter + ": Ports checked" + "\r\n";
                    line_counter++;
                    if (pump_port_connection == false)
                    {
                        try
                        {
                            //proba utworzenia danych portu
                            create_pump_connection();
                            //proba otwarcia portu
                            open_pump_connection();
                            //wyswietlanie logow
                            this.textBox3.Text = this.textBox3.Text + line_counter + ": Pump connected" + "\r\n";
                            line_counter++;
                        }
                        catch
                        {
                            //wyswietlanie logow
                            this.textBox3.Text = this.textBox3.Text + line_counter + ": Could not connect to pump" + "\r\n";
                            line_counter++;
                        }
                    }
                    else
                    {
                        //wyswietlanie logow
                        this.textBox3.Text = this.textBox3.Text + line_counter + ": Pump connected" + "\r\n";
                        line_counter++;
                    }
                    if (ploter_port_connection == false)
                    {
                        try
                        {
                            //proba utworzenia danych portu
                            create_ploter_connection();
                            //proba otwarcia portu
                            open_ploter_connection();
                            //wyswietlanie logow
                            this.textBox3.Text = this.textBox3.Text + line_counter + ": Ploter connected" + "\r\n";
                            line_counter++;
                        }
                        catch
                        {
                            //wyswietlanie logow
                            this.textBox3.Text = this.textBox3.Text + line_counter + ": Could not connect to ploter" + "\r\n";
                            line_counter++;
                        }
                    }
                    else
                    {
                        //wyswietlanie logow
                        this.textBox3.Text = this.textBox3.Text + line_counter + ": Ploter connected" + "\r\n";
                        line_counter++;
                    }
                }
                catch
                {
                    //wyswietlanie logow
                    this.textBox3.Text = this.textBox3.Text + line_counter + ": Could not check connection" + "\r\n";
                    line_counter++;
                }
            }
        }

        //Start - Button
        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                if(pump_port_connection == false)
                {
                    //proba utworzenia danych portu
                    create_pump_connection();
                    //proba otwarcia portu
                    open_pump_connection();
                }
                
                try
                {
                    if (ploter_port_connection == false)
                    {
                        //proba utworzenia danych portu
                        create_ploter_connection();
                        //proba otwarcia portu
                        open_ploter_connection();
                    }

                    //pobiera tekst z textbox1
                    string_buffor_3 = this.textBox1.Text;

                    //mierzy dlugosc tekstu
                    lenght_of_text = this.textBox1.Text.Length;

                    //zlicza ile lin wystepuje w tekscie
                    num_of_lines = this.textBox1.Lines.Count();

                    //podzial tekstu na osobne linie
                    for (int i = 0; i < num_of_lines - 1; i++)
                    {
                        pos_of_new_line = string_buffor_3.IndexOf("\r\n") + 2;
                        string_buffor_2 = string_buffor_3.Substring(0, pos_of_new_line);
                        line[i] = string_buffor_2;
                        lenght_of_text = lenght_of_text - pos_of_new_line;
                        string_buffor_3 = string_buffor_3.Substring(pos_of_new_line, lenght_of_text);

                        //jezeli jest komenda G00 lub G01 w lini
                        if (line[i].IndexOf("G00", 0, StringComparison.OrdinalIgnoreCase) != -1 ||
                            line[i].IndexOf("G01", 0, StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            //jezeli jest komenda G90 lub G91 w lini
                            if (line[i].IndexOf("G90", 0, StringComparison.OrdinalIgnoreCase) != -1 ||
                                line[i].IndexOf("G91", 0, StringComparison.OrdinalIgnoreCase) != -1)
                            {
                                //jezeli nie ma komendy F i (X lub Y) w lini
                                if (line[i].IndexOf("F", 0, StringComparison.OrdinalIgnoreCase) == -1 &&
                                    (line[i].IndexOf("X", 0, StringComparison.OrdinalIgnoreCase) == -1 ||
                                    line[i].IndexOf("Y", 0, StringComparison.OrdinalIgnoreCase) == -1))
                                {
                                    //wyswietlanie logow
                                    this.textBox3.Text = this.textBox3.Text + line_counter + ": Wrong data format for line: " + (i + 1).ToString() + "\r\n";
                                    line_counter++;
                                }
                                //jezeli jest komenda F, X lub Y w lini
                                else
                                {
                                    //Jesli ruch odbywa sie zarowno w kierunku osi X i Y
                                    if (line[i].IndexOf("X", 0, StringComparison.OrdinalIgnoreCase) != -1 &&
                                        line[i].IndexOf("Y", 0, StringComparison.OrdinalIgnoreCase) != -1)
                                    {
                                        x_move = true;
                                        y_move = true;
                                    }
                                    //Jesli ruch odbywa sie tylko w kierunku osi X
                                    else if (line[i].IndexOf("X", 0, StringComparison.OrdinalIgnoreCase) != -1)
                                    {
                                        x_move = true;
                                        y_move = false;
                                    }
                                    //Jesli ruch odbywa sie tylko w kierunku osi Y
                                    else if (line[i].IndexOf("Y", 0, StringComparison.OrdinalIgnoreCase) != -1)
                                    {
                                        x_move = false;
                                        y_move = true;
                                    }
                                    //Jesli ruch nie odbywa sie w kierunku osi X ani Y
                                    else
                                    {
                                        x_move = false;
                                        y_move = false;
                                    }
                                    count_path(line[i], i);
                                }
                            }
                            else
                            {
                                //wyswietlanie logow
                                this.textBox3.Text = this.textBox3.Text + line_counter + ": Wrong data format for line: " + (i + 1).ToString() + "\r\n";
                                line_counter++;
                            }
                        }
                        else
                        {
                            //wyswietlanie logow
                            this.textBox3.Text = this.textBox3.Text + line_counter + ": Wrong data format for line: " + (i + 1).ToString() + "\r\n";
                            line_counter++;
                        }
                    }

                    //jezeli nie ustawiono pozycji startowej
                    if (set_start_pos == false)
                    {
                        Home_X_pos = 0;
                        Home_Y_pos = 0;
                    }

                    //jesli jest polaczenie z ploterem i pompa
                    if (ploter_port_connection == true && pump_port_connection == true)
                    {
                        //idzie do kata
                        ploter_port.WriteLine(String.Format("F1000 G91 G00 X-" + (x_pos + 195).ToString() + " Y-" + (y_pos + 122).ToString()));
                        //wyswietlanie logow
                        this.textBox3.Text = this.textBox3.Text + line_counter + ": Going to corner" + "\r\n";
                        line_counter++;
                        //idzie do pozycji Home
                        ploter_port.WriteLine(String.Format("F1000 G91 G00 X" + (Home_X_pos + 195).ToString() + " Y" + (Home_Y_pos + 122).ToString()));
                        //wyswietlanie logow
                        this.textBox3.Text = this.textBox3.Text + line_counter + ": Going to Home position" + "\r\n";
                        line_counter++;

                        System.Threading.Thread.Sleep(18000);

                        x_pos = Home_X_pos;
                        y_pos = Home_Y_pos;

                        //czasu pracy pompy [ms]
                        int msLimit = (Convert.ToInt32(0));

                        //wykonaj x razy
                        for (int i = 0; i <= repeat; i++)
                        {
                            //leci po kazdej linii
                            for (int j = 0; j < num_of_lines - 1; j++)
                            {
                                if (set_speed == true)
                                {
                                    pos_of_F_in_line = line[j].IndexOf("F");
                                    string_buffor_1 = line[j].Substring(0, pos_of_F_in_line);
                                    string_buffor_2 = line[j].Substring(pos_of_F_in_line, line[j].Length - pos_of_F_in_line);
                                    end_pos_of_F_value_in_line = string_buffor_2.IndexOf(" ");
                                    string_buffor_3 = line[j].Substring(end_pos_of_F_value_in_line, line[j].Length - end_pos_of_F_value_in_line);
                                    line[j] = string.Empty;
                                    line[j] = string_buffor_1 + speed.ToString() + string_buffor_3;
                                    string_buffor_1 = string.Empty;
                                    string_buffor_2 = string.Empty;
                                    string_buffor_3 = string.Empty;
                                }

                                pos_of_F_in_line = line[j].IndexOf("F");
                                string_buffor_2 = line[j].Substring(pos_of_F_in_line, line[j].Length - pos_of_F_in_line);
                                end_pos_of_F_value_in_line = string_buffor_2.IndexOf(" ");
                                string_buffor_3 = line[j].Substring(pos_of_F_in_line + 1, end_pos_of_F_value_in_line - 1);
                                intiger_buffor_1 = int.Parse(string_buffor_3);
                                msLimit = Convert.ToInt32((move_value[j] / intiger_buffor_1) * 1000 * 60);
                                intiger_buffor_1 = 0;
                                string_buffor_2 = string.Empty;
                                string_buffor_3 = string.Empty;

                                if (i == repeat)
                                {
                                    ploter_port.WriteLine(String.Format(line[j]));
                                    //wyswietlanie logow
                                    this.textBox3.Text = this.textBox3.Text + line_counter + ": Execute: " + line[j].ToString();
                                    line_counter++;
                                }
                                else
                                {
                                    if (i == 0 && j == 0)
                                    {
                                        //wyswietlanie logow
                                        this.textBox3.Text = this.textBox3.Text + line_counter + ": Started pumping" + "\r\n";
                                        line_counter++;
                                    }
                                    try
                                    {
                                        //Start pracy pompy
                                        pump_port.WriteLine(String.Format("RUN\r\n"));
                                    }
                                    catch
                                    {
                                        //wyswietlanie logow
                                        this.textBox3.Text = this.textBox3.Text + line_counter + ": Pump operation RUN failed" + "\r\n";
                                        line_counter++;
                                    }
                                    ploter_port.WriteLine(String.Format(line[j]));
                                    //wyswietlanie logow
                                    this.textBox3.Text = this.textBox3.Text + line_counter + ": Execute: " + line[j].ToString();
                                    line_counter++;

                                    System.Threading.Thread.Sleep(msLimit);
                                    
                                    //Pauza pracy pompy do nastepnego wywolania("STP"x1)
                                    pump_port.WriteLine(String.Format("STP\r\n"));
                                }
                            }
                        }

                        //idzie na bok
                        ploter_port.WriteLine(String.Format("F1000 G91 G00 X-50 Y-50"));

                        try
                        {
                            //Stop pracy pompy ("STP"x2)
                            pump_port.WriteLine(String.Format("STP\r\n"));
                            //wyswietlanie logow
                            this.textBox3.Text = this.textBox3.Text + line_counter + ": Stoped pumping" + "\r\n";
                            line_counter++;
                        }
                        catch
                        {
                            //Stop pracy pompy ("STP"x2)
                            pump_port.WriteLine(String.Format("STP\r\n"));
                            //wyswietlanie logow
                            this.textBox3.Text = this.textBox3.Text + line_counter + ": Could not stop pump" + "\r\n";
                            line_counter++;
                        }
                        //wyswietlanie logow
                        this.textBox3.Text = this.textBox3.Text + line_counter + ": Job finished" + "\r\n";
                        line_counter++;
                    }
                    else
                    {
                        //wyswietlanie logow
                        this.textBox3.Text = this.textBox3.Text + line_counter + ": No connection with pump or ploter, check connection" + "\r\n";
                        line_counter++;
                    }
                }
                catch
                {
                    //wyswietlanie logow
                    this.textBox3.Text = this.textBox3.Text + line_counter + ": Ploter is disconnected " + "\r\n";
                    line_counter++;
                }
            }
            catch
            {
                //wyswietlanie logow
                this.textBox3.Text = this.textBox3.Text + line_counter + ": Pump is disconnected " + "\r\n";
                line_counter++;
            }
        }

        //Stop - Button
        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                if (pump_port_connection == false)
                {
                    //proba utworzenia danych portu
                    create_pump_connection();
                    //proba otwarcia portu
                    open_pump_connection();
                }
                else
                {
                    pump_port.Open();
                }

                try
                {
                    if (ploter_port_connection == false)
                    {
                        //proba utworzenia danych portu
                        create_ploter_connection();
                        //proba otwarcia portu
                        open_ploter_connection();
                    }
                    else
                    {
                        ploter_port.Open();
                    }

                    try
                    {
                        pump_port.WriteLine(String.Format("STP\r\n"));
                        pump_port.WriteLine(String.Format("STP\r\n"));
                        //wyswietlanie logow
                        this.textBox3.Text = this.textBox3.Text + line_counter + ": Stoped pump work" + "\r\n";
                        line_counter++;
                        try
                        {

                            ploter_port.WriteLine(String.Format("M99"));
                            //wyswietlanie logow
                            this.textBox3.Text = this.textBox3.Text + line_counter + ": Stoped ploter work" + "\r\n";
                            line_counter++;
                        }
                        catch
                        {
                            //wyswietlanie logow
                            this.textBox3.Text = this.textBox3.Text + line_counter + ": Ploter operation (STOP) failed" + "\r\n";
                            line_counter++;
                        }
                    }
                    catch
                    {
                        //wyswietlanie logow
                        this.textBox3.Text = this.textBox3.Text + line_counter + ": Pump operation (STOP) failed" + "\r\n";
                        line_counter++;
                    }
                    ploter_port.Close();
                }
                catch
                {
                    //wyswietlanie logow
                    this.textBox3.Text = this.textBox3.Text + line_counter + ": Ploter is disconnected " + "\r\n";
                    line_counter++;
                }
                pump_port.Close();
            }
            catch
            {
                //wyswietlanie logow
                this.textBox3.Text = this.textBox3.Text + line_counter + ": Pump is disconnected " + "\r\n";
                line_counter++;
            }
        }

        //Wybierz Template - Button
        private void button6_Click(object sender, EventArgs e)
        {
            show_submenu();
        }

        //Import pliku Gcode - Button (.txt file)
        private void button7_Click(object sender, EventArgs e)
        {
            import_file(path_importer);
        }

        //Powtorzenia - Label
        private void label9_Click(object sender, EventArgs e)
        {

        }

        //Powtorzenia - Button
        private void button14_Click(object sender, EventArgs e)
        {
            try
            {
                //parse z string do int
                repeat = int.Parse(this.textBox2.Text);
                //wyswietlanie logow
                this.textBox3.Text = this.textBox3.Text + line_counter + ": Repeat parameter has been changed to: " + repeat.ToString() + " times" + "\r\n";
                line_counter++;
            }
            catch
            {
                //wyswietlanie logow
                this.textBox3.Text = this.textBox3.Text + line_counter + ": Repeat parameter imput wrong format" + "\r\n";
                line_counter++;
            }
        }

        //Powtorzenia - TextBox
        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        //Okno edycji komend - textbox
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.ScrollBars = ScrollBars.Vertical;
        }

        //template 1 - Button
        private void button16_Click(object sender, EventArgs e)
        {
            import_file(template_1);
        }

        //template 2 - Button
        private void button17_Click(object sender, EventArgs e)
        {
            import_file(template_2);
        }

        //template 3 - Button
        private void button18_Click(object sender, EventArgs e)
        {
            import_file(template_3);
        }

        //template 4 - Button
        private void button19_Click(object sender, EventArgs e)
        {
            import_file(template_4);
        }

        //Wyczysc okno edycji komend - Button
        private void button20_Click(object sender, EventArgs e)
        {
            this.textBox1.Clear();
        }

        //Load Form1
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        //Close Form1
        private void Form1_Closed(object sender, System.EventArgs e)
        {
            if (pump_port_connection == false)
            {
                //tworzy polaczenia serial z pompa
                create_pump_connection();
                //probuje nawiazac polaczenie z pompa
                open_pump_connection();
            }
            if (ploter_port_connection == false)
            {
                //tworzy polaczenia serial z ploterem
                create_ploter_connection();
                //probuje nawiazac polaczenie z ploterem
                open_ploter_connection();
            }

            //zamyka polaczenia
            pump_port.Close();
            ploter_port.Close();
        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        //Zaczyna pompowac - Button
        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                if (pump_port_connection == false)
                {
                    //proba utworzenia danych portu
                    create_pump_connection();
                    //proba otwarcia portu
                    open_pump_connection();
                }

                try
                {
                    //Start pracy pompy
                    pump_port.WriteLine(String.Format("RUN\r\n"));
                    //wyswietlanie logow
                    this.textBox3.Text = this.textBox3.Text + line_counter + ": Started pumping" + "\r\n";
                    line_counter++;
                }
                catch
                {
                    //wyswietlanie logow
                    this.textBox3.Text = this.textBox3.Text + line_counter + ": Pump operation RUN failed" + "\r\n";
                    line_counter++;
                }
            }
            catch
            {
                //wyswietlanie logow
                this.textBox3.Text = this.textBox3.Text + line_counter + ": Pump is disconnected " + "\r\n";
                line_counter++;
            }
        }

        //Przestaje pompowac - Button
        private void button11_Click(object sender, EventArgs e)
        {
            try
            {
                if (pump_port_connection == false)
                {
                    //proba utworzenia danych portu
                    create_pump_connection();
                    //proba otwarcia portu
                    open_pump_connection();
                }

                try
                {
                    //Stop pracy pompy ("STP"x1)
                    pump_port.WriteLine(String.Format("STP\r\n"));
                    //wyswietlanie logow
                    this.textBox3.Text = this.textBox3.Text + line_counter + ": Stoped pumping" + "\r\n";
                    line_counter++;
                }
                catch
                {
                    //wyswietlanie logow
                    this.textBox3.Text = this.textBox3.Text + line_counter + ": Pump operation STOP failed" + "\r\n";
                    line_counter++;
                }
            }
            catch
            {
                //wyswietlanie logow
                this.textBox3.Text = this.textBox3.Text + line_counter + ": Pump is disconnected " + "\r\n";
                line_counter++;
            }
        }
    }
}
