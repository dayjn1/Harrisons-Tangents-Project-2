// File name:                   Form1.cs
// Project name:                Project 2 - Harrison's Tangents
// ---------------------------------------------------------------------------
// Creator’s name:              Janine Day
// Edited By:                   Janine Day, Jason Middlebrook, 
// Course-Section:              CSCI-4717
// Creation Date:               02/17/2022
// ---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Project2_HT
{
    /**
    * Class Name:       Tangents
    * Class Purpose:    GUI code for fetching input, processing input, running simulation, and (hopefully) save info into textfile
    *
    * <hr>
    * Date created: 02/17/2022
    * @Janine Day
    */
    public partial class Tangents : Form
    {
        List<Instruction> Input_Instructions = new List<Instruction>();         // Creates a list of Instruction class types -JND
        List<Instruction> Save_Stats = new List<Instruction>();
        Stack<Instruction> Fetch = new Stack<Instruction>();                    // Creates the stacks for pipeline process -JND
        Stack<Instruction> Decode = new Stack<Instruction>();
        Stack<Instruction> Execute = new Stack<Instruction>();
        Stack<Instruction> Memory = new Stack<Instruction>();
        Stack<Instruction> Register = new Stack<Instruction>();
        int cycleCount = 0;                                                     // Counts the number of cycles
        List<String> usedRegisters = new List<string>(); //store stale registers  
        int hazardCount = 0;        //count hazards
        int SimulationCount;
        int time = 500;

        /**
        * Method Name: Tangents()
        * Method Purpose: Automatically Generated code to initialize GUI
        *
        * <hr>
        * Date created: 02/17/2022
        *
        * <hr>
        */
        public Tangents()
        {
            InitializeComponent();
        }

        /**
        * Method Name: openToolStripMenuItem_Click(object, EventArgs)
        * Method Purpose: To use an open file dialog which allows users which text file they would like to process
        *
        * <hr>
        * Date created: 02/17/2022
        * @Janine Day
        * <hr>
        * @param object - sender
        * @param EventArgs - e
        */
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog OpenDlg = new OpenFileDialog();                      // Declares and initializes OpenFileDialog
            if (DialogResult.Cancel != OpenDlg.ShowDialog())                    // If file was successfully opened
            {
                string fileName = OpenDlg.FileName;                             // Get file name
                StreamReader f = new StreamReader(fileName);                    // Declares and initializes Streamreader using file name

                while (f.Peek() != -1)                                          // While there is more to read for the file
                {
                    string inputData = f.ReadLine();                            //Declares inputData so lines can be read from input

                    //try to parse one line of input, converting hexadecimal to int and sending to disassembler -H, JM
                    int input;
                    bool valid = Int32.TryParse(inputData, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out input);
                    if (valid)
                    {
                        Input_Instructions.Add(new Instruction(input));         // Creates instructions and adds them to list -JND
                        Save_Stats.Add(new Instruction(input));
                    }
                    else
                        Console.WriteLine("Invalid parse");

                }//end while
                label8.Text = "Loaded";
                cycleCount = 0;
                hazardCount = 0;
                cycleLabel.Text = cycleCount.ToString();
                label7.Text = hazardCount.ToString();
            }//end if

        }

        /**
        * Method Name: StartButton_Click(object, EventArgs)
        * Method Purpose: Starts simulation once event occurs
        *
        * <hr>
        * Date created: 02/17/2022
        * @Janine Day
        * <hr>
        * @param object - sender
        * @param EventArgs - e
        */
        private void StartButton_Click(object sender, EventArgs e)
        {
            label8.Text = "Processing...";
            Simulation();
            label8.Text = "Finished";
        }

        /**
        * Method Name: Simulation()
        * Method Purpose: Algorithm to run pipeline process. Uses global stacks and user input
        *
        * <hr>
        * Date created: 02/17/2022
        * @Janine Day
        * <hr>
        */

        public void Simulation()
        {
            for (int i = 0; i < this.Input_Instructions.Count; i++)
            {

                while (this.SimulationCount < this.Input_Instructions.Count)
                {
                    CountUpdate();
                    UpdateAndDelay();

                    if (this.Register.Count > 0)
                    {

                        Instruction wb = this.Register.Pop();
                        usedRegisters.Remove(wb.DestReg);
                        this.RegisterBox.Text = "";
                    }

                    if (this.Memory.Count > 0)
                    {
                        ProcessRegister();
                    }

                    if (this.Execute.Count > 0)
                    {
                        ProcessMemory();
                    }

                    if (this.Decode.Count > 0)
                    {
                        ProcessExecute();
                    }

                    if (this.Fetch.Count > 0)
                    {

                        ProcessDecode();
                    }

                    if (this.SimulationCount < this.Input_Instructions.Count && this.Fetch.Count == 0)
                    {
                        PushFetch(this.Input_Instructions[this.SimulationCount]);
                        this.SimulationCount++;
                    }

                }

                // clean up pipeline

                while (this.Fetch.Count != 0 || this.Decode.Count != 0 || this.Execute.Count != 0 || this.Memory.Count != 0 || this.Register.Count != 0)
                {

                    if (this.Register.Count > 0)
                    {
                        this.Register.Pop();
                        this.RegisterBox.Text = "";
                        if (this.Fetch.Count == 0 && this.Decode.Count == 0 && this.Execute.Count == 0 && this.Memory.Count == 0)
                            return;
                    }

                    CountUpdate();
                    UpdateAndDelay();

                    if (this.Memory.Count > 0)
                    {

                        ProcessRegister();
                    }

                    if (this.Execute.Count > 0)
                    {
                        ProcessMemory();
                    }

                    if (this.Decode.Count > 0)
                    {
                        ProcessExecute();
                    }

                    if (this.Fetch.Count > 0)
                    {
                        ProcessDecode();
                    }



                    if (this.Fetch.Count > 0)
                    {
                        //check for invalid before decode
                        //.Peek() method to view instruction currently in Fetch stack - H
                        //Instruction tempF = this.Fetch.Peek();

                        //if (!(Instruction.InstructionSet.Contains(tempF))) --keeps entering the if even when the instruction is valid
                        //    InvalidFound();

                        ProcessDecode();
                    }
                }

            }
        }//end simulation

        /**
        * Method Name: InvalidFound()
        * Method Purpose: Method to pop all stacks and close/crash the program when illegal instruction is found
        *
        * <hr>
        * Date created: 03/07/2022
        * @Hannah Taylor
        * <hr>
        */
        public void InvalidFound()
        {
            //check each stack 
            if (this.Fetch.Count > 0)
            {
                this.Fetch.Pop();
                //update text
                FetchBox.Text = "";
                Update();
            }
            if (this.Decode.Count > 0)
            {
                this.Decode.Pop();
                //update text
                DecodeBox.Text = "";
                Update();
            }
            if (this.Execute.Count > 0)
            {
                this.Execute.Pop();
                //update text
                ExecuteBox.Text = "";
                Update();
            }
            if (this.Memory.Count > 0)//memory
            {
                this.Memory.Pop();
                //update text
                MemoryBox.Text = "";
                Update();
            }
            if (this.Register.Count > 0)                //register
            {
                this.Register.Pop();
                //update text
                RegisterBox.Text = "";
                Update();
            }

            //method call to close Gui??
            System.Windows.Forms.Application.Exit();
        }

        /**
        * Method Name: InvalidFound()
        * Method Purpose: Method to finish simulation when halt reaches Execute before it is popped off
        *
        * <hr>
        * Date created: 03/07/2022
        * @Hannah Taylor
        * <hr>
        */
        public void HaltFound()
        {
            //clear any instructions loaded after halt
            if(this.Fetch.Count > 0)
            {
                this.Fetch.Pop();
                //update text
                FetchBox.Text = "";
                Update();
            }
            if (this.Decode.Count > 0)
            {
                this.Decode.Pop();
                //update text
                DecodeBox.Text = "";
                Update();
            }

            //keep going if anything in memory or writeback (halt is in Execute)

        }
         

        /**
        * Method Name: KeepGoing(int)
        * Method Purpose: Process a single cycle while a stack is stalling
        *
        * <hr>
        * Date created: 03/06/2022
        * @Janine Day
        * <hr>
        * @param i - used to determine which stack is stalling
        */
        public void KeepGoing(int i)
        {
            if (i == 1) //decode stall
            {
                if (this.Register.Count > 0)    // register for one cycle
                {
                    RegisterCycle();
                    UpdateAndDelay();
                }

                if (this.Memory.Count > 0)      // memory for one cycle
                {
                    MemoryCycle();
                    UpdateAndDelay();
                }

                if (this.Execute.Count > 0)     // Execute for one cycle
                {
                    ExecuteCycle();
                    UpdateAndDelay();
                }

                // fetch for one cycle
                if (this.Fetch.Count == 0 && (this.SimulationCount < this.Input_Instructions.Count))
                {
                    FetchCycle();
                    UpdateAndDelay();
                }

            }
            else if (i == 2) //execute stall
            {
                if (this.Register.Count > 0)        // register for one cycle
                {
                    RegisterCycle();
                    UpdateAndDelay();
                }

                if (this.Memory.Count > 0)          // memory for one cycle
                {
                    MemoryCycle();
                    UpdateAndDelay();
                }
                
                if (this.Fetch.Count == 0 && (this.SimulationCount) < this.Input_Instructions.Count)
                {
                    FetchCycle();
                    UpdateAndDelay();
                }

            }
            else if (i == 3) //memory stall
            {
                if (this.Register.Count > 0)
                {
                    RegisterCycle();
                    UpdateAndDelay();
                }

                if (this.Execute.Count == 1)
                {
                    ExecuteCycle();
                    UpdateAndDelay();

                }
                else if (this.Execute.Count == 0 && this.Decode.Count == 1)
                {
                    DecodeCycle();
                    UpdateAndDelay();
                }

                if (this.Fetch.Count == 0 && (this.SimulationCount < this.Input_Instructions.Count))
                {
                    FetchCycle();
                    UpdateAndDelay();
                }

            }
        }

        public void RegisterCycle()
        {
            this.Register.Pop();
            this.RegisterBox.Text = "";
        }

        public void MemoryCycle()
        {
            Instruction temp = this.Memory.Peek();

            if (temp.MemoryCC == 0)
            {
                temp = this.Memory.Pop();
                this.MemoryBox.Text = "";

                if (temp.RegisterCC > 0)
                {
                    PushRegister(temp);
                }
            }
            else if (temp.MemoryCC > 0)
            {
                temp.MemoryCC--;
            }
        }

        public void ExecuteCycle()
        {
            Instruction temp = this.Execute.Peek();

            if (temp.ExecuteCC == 0)
            {
                if (temp.ExecuteCC == 0 && temp.MemoryCC == 0 && temp.RegisterCC == 0)
                {
                    this.Execute.Pop();
                }
                else if (temp.MemoryCC > 0 && this.Memory.Count == 0)
                {
                    this.Execute.Pop();
                    PushMemory(temp);
                    MemoryText(temp);
                }
                else if (temp.RegisterCC > 0 && this.Register.Count == 0)
                {
                    this.Execute.Pop();
                    this.ExecuteBox.Text = "";
                    PushRegister(temp);
                    RegisterText(temp);
                }
                else if (temp.ExecuteCC > 0)
                    temp.ExecuteCC--;
            }
        }

        public void DecodeCycle()
        {
            Instruction temp = this.Decode.Peek();
            if (temp.DecodeCC == 0)
            {
                this.Decode.Pop();
                this.DecodeBox.Text = "";
                PushExecute(temp);
            }
            else
            {
                temp.DecodeCC--;
            }

        }

        public void FetchCycle()
        {
            if(this.Fetch.Count == 0)
            {
                PushFetch(this.Input_Instructions[this.SimulationCount]);
                this.SimulationCount++;
            }
            else if (this.Decode.Count == 0 && this.Fetch.Count > 0)     // decode for one cycle
            {
                Instruction temp = this.Fetch.Pop();
                PushDecode(temp);
            }
        }



        
        // See if an operand register in an instruction is being used already, and stall until it is free.
        //Avery 
        public void CompareOpRegisters(Instruction i)
        {
            //check if its in the stale registers
            if (usedRegisters.Contains(i.Reg1) && i.Reg1 != null || usedRegisters.Contains(i.Reg2) && i.Reg2 != null)
            {
                hazardCount++;
                label7.Text = hazardCount.ToString();
                CountUpdate(); //go stall
            }
            else
            {
                return; //if not, leave
            }
            CompareOpRegisters(i); //try again otherwise
        }

        public void ProcessDecode()
        {
            Instruction i = this.Fetch.Pop();
            this.FetchBox.Text = "";
            CompareOpRegisters(i);
            if(i.writeBack == true)
            {
                CheckRegisters(i);
            }
            if (i.DecodeCC != 0)
            {
                PushDecode(i);

                //CountUpdate();
                UpdateAndDelay();

                while (i.DecodeCC > 0)
                {
                    i.DecodeCC--;

                    CountUpdate();
                    UpdateAndDelay();
                }
            }
        }

        public void ProcessExecute()
        {
            Instruction i = this.Decode.Pop();
            this.DecodeBox.Text = "";
            if (i.ExecuteCC != 0)
            {
                PushExecute(i);
                UpdateAndDelay();


                while (i.ExecuteCC > 0)
                {
                    i.ExecuteCC--;

                    CountUpdate();
                    UpdateAndDelay();
                }
            }
        }

        public void ProcessMemory()
        {
            Instruction i = this.Execute.Pop();
            this.ExecuteBox.Text = "";

            if (i.MemoryCC != 0)
            {
                PushMemory(i);

                //CountUpdate();
                UpdateAndDelay();

                while (i.MemoryCC > 0)
                {
                    i.MemoryCC--;

                    CountUpdate();
                    UpdateAndDelay();
                }
            }
            else if(i.RegisterCC != 0)
            {
                PushRegister(i);

                //CountUpdate();
                UpdateAndDelay();


                while (i.RegisterCC > 0)
                {
                    i.RegisterCC--;

                    CountUpdate();
                    UpdateAndDelay();
                }
            }
        }

        public void ProcessRegister()
        {
            Instruction i = this.Memory.Pop();
            this.MemoryBox.Text = "";
            
            if (i.RegisterCC != 0)
            {
                PushRegister(i);

                UpdateAndDelay();


                while (i.RegisterCC > 0)
                {
                    i.RegisterCC--;

                    CountUpdate();
                    UpdateAndDelay();
                }
                usedRegisters.Clear();  //clear registers when no longer in use
            }
          
        }
        /// <summary>Accepts an instruction and checks if its registers are available.
        /// On fail it waits until the registers are available. Used to get the registers ready to push.</summary>
        /// AM
        /// <param name="i">Instruction passed in from Process Registers</param>
        public void CheckRegisters(Instruction i)
        {

            //see if register is available by checking the usedRegisters list
            if (!usedRegisters.Contains(i.DestReg))
            {
                usedRegisters.Add(i.DestReg);
            }
            else//if the register is in use already, wait until it is not.
            {
                hazardCount++;
                label7.Text = hazardCount.ToString();
                Task.Delay(time).Wait();
                usedRegisters.Remove(i.DestReg);
                CheckRegisters(i);
            }

        }
        public void CountUpdate()
        {
            this.cycleCount++;
            cycleLabel.Text = cycleCount.ToString();
        }

        public void UpdateAndDelay()
        {
            Update();
            Task.Delay(500).Wait();
        }

        public void PushFetch(Instruction i)
        {
            this.Fetch.Push(i);
            i.FetchCC--;
            FetchText(i);
        }

        public void PushDecode(Instruction i)
        {
            this.Decode.Push(i);
            i.DecodeCC--;
            DecodeText(i);
        }

        public void PushExecute(Instruction i)
        {
            this.Execute.Push(i);
            i.ExecuteCC--;
            ExecuteText(i);
        }

        public void PushMemory(Instruction i)
        {
            this.Memory.Push(i);
            i.MemoryCC--;
            MemoryText(i);
        }

        public void PushRegister(Instruction i)
        {
            this.Register.Push(i);
            i.RegisterCC--;
            RegisterText(i);
        }


        /**
        * Method Name: RegisterText(Instruction)
        * Method Purpose: Updates RegisterText content
        *
        * <hr>
        * Date created: 02/17/2022
        * @Janine Day
        * <hr>
        * @param Instruction - provides info for visuals
        */
        public void RegisterText(Instruction i)
        {
            RegisterBox.Text = i.Mnemonic;
        }

        /**
        * Method Name: MemoryText(Instruction)
        * Method Purpose: Updates MemoryText content
        *
        * <hr>
        * Date created: 02/17/2022
        * @Janine Day
        * <hr>
        * @param Instruction - provides info for visuals
        */
        public void MemoryText(Instruction i)
        {
            MemoryBox.Text = i.Mnemonic;
        }

        /**
        * Method Name: ExecuteText(Instruction)
        * Method Purpose: Updates ExecuteText content
        *
        * <hr>
        * Date created: 02/17/2022
        * @Janine Day
        * <hr>
        * @param Instruction - provides info for visuals
        */
        public void ExecuteText(Instruction i)
        {
            ExecuteBox.Text = i.Mnemonic;
        }

        /**
        * Method Name: DecodeText(Instruction)
        * Method Purpose: Updates DecodeText content
        *
        * <hr>
        * Date created: 02/17/2022
        * @Janine Day
        * <hr>
        * @param Instruction - provides info for visuals
        */
        public void DecodeText(Instruction i)
        {
            DecodeBox.Text = i.Mnemonic;
        }

        /**
        * Method Name: FetchText(Instruction)
        * Method Purpose: Updates FetchText content
        *
        * <hr>
        * Date created: 02/17/2022
        * @Janine Day
        * <hr>
        * @param Instruction - provides info for visuals
        */
        public void FetchText(Instruction i)
        {
            FetchBox.Text = i.Mnemonic;
        }

        public static string dirParameter = AppDomain.CurrentDomain.BaseDirectory + @"\saveOut.txt";
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //OpenFileDialog SaveDlg = new OpenFileDialog();
            //string fileN = SaveDlg.FileName;
            FileStream fParameter = new FileStream(dirParameter, FileMode.Create, FileAccess.Write);
            StreamWriter filewrite = new StreamWriter(fParameter);

            filewrite.WriteLine("   Instruction    |    Fetch    |    Decode    |    Execute    |    Memory    |    WriteBack ");
            filewrite.WriteLine("______________________________________________________________________________________________");

            int f, d, exe, m, w;
            int fetch = 0, 
                decodeS = 0, 
                decodeE = 0, 
                executeS = 0, 
                executeE = 0, 
                memS = 0, 
                memE = 0, 
                writeB = 0;
            //int lastHit = 0;
            bool memUsedBefore = false;
            bool decodeStall = false;

            string decode, exec, memo = "", stringWB;
            //int decodeStallE = -1, executeStallE = -1, memStallE = -1;

            for (int i = 0; i < Save_Stats.Count; i++)
            {
                f = Save_Stats[i].FetchCC;
                d = Save_Stats[i].DecodeCC;
                exe = Save_Stats[i].ExecuteCC;
                m = Save_Stats[i].MemoryCC;
                w = Save_Stats[i].RegisterCC;

                //in every case, fetch is calculated the same
                fetch = f + i;

                //decode
                if(i == 0)
                {
                    decodeS = fetch + 1;
                    decodeE = fetch + d;

                    if (d > 1)
                    {
                        decode = decodeS + " - " + decodeE;
                        decodeStall = true;
                    }
                    else
                        decode = decodeS.ToString();
                }
                else
                {
                    decodeS = decodeE + 1;  //decodeE should still contain the value from the prev iteration
                    decodeE += 1; //assume one cycle to decode

                    if (d > 1)
                    {
                        decodeE = (d - 1) + decodeS; //update if actually takes more than one cycle
                        decode = decodeS + " - " + decodeE;
                        decodeStall = true;
                    }
                    else
                    {
                        decode = decodeS.ToString();
                    }
                }

                //execute
                if (i == 0 || decodeStall == true)
                {
                    executeS = decodeE + 1;
                    executeE = decodeE + 1; //assume one cycle

                    if (exe > 1)
                    {
                        executeE = (exe - 1) + executeS; //update if more than one
                        exec = executeS + " - " + executeE;
                    }
                    else
                        exec = executeS.ToString();

                    decodeStall = false; //reset for next decode calculation
                }
                else
                {
                    executeS = executeE + 1; //can safely calc from prev execute end cycle
                    executeE += 1; //assume one cycle to execute

                    if (exe > 1)
                    {
                        executeE = (exe - 1) + executeS; //update if more than one
                        exec = executeS + " - " + executeE;
                    }
                    else
                        exec = executeS.ToString();
                }
                

                //memory
                if (m != 0) //if we need to access memory
                {
                    if (i == 0)
                    {
                        memS = executeE + 1;
                        memE = (m - 1) + memS;
                        memo = memS + " - " + memE;
                        memUsedBefore = true;
                    }
                    else
                    {
                        if (memUsedBefore)
                        {
                            if(memE < executeE)
                            {
                                memS = executeE + 1;
                                memE = (m - 1) + memS;
                                memo = memS + " - " + memE;
                                memUsedBefore = true;
                            }
                            else
                            {
                                memS = memE + 1;
                                memE = (m - 1) + memS;
                                memo = memS + " - " + memE;
                                memUsedBefore = true;
                            }
                        }
                        else
                        {
                            memS = executeE + 1;
                            memE = (m - 1) + memS;
                            memo = memS + " - " + memE;
                            memUsedBefore = true;
                        }
                    }
                }
                else
                    memo = " ";

                //^end memory section

                //register writeback -- assuming never takes longer than one cycle
                if (w != 0)
                {
                    if (m != 0)
                    {
                        writeB = memE + 1;
                        stringWB = writeB.ToString();
                    }
                    else
                    {
                        writeB = executeE + 1; //increment counter from execute stage if memory was not touched
                        stringWB = writeB.ToString();
                    }
                }
                else
                {
                    stringWB = " ";
                }


                filewrite.WriteLine("     " + Save_Stats[i].Mnemonic.PadRight(5, ' ') + "              " + 
                                   (fetch) + "             " +
                                   (decode.PadRight(7, ' ')) + "         " +
                                   (exec) + "              " +
                                   (memo.PadRight(9, ' ')) + "       " +
                                   (stringWB) + "          ");


                
                


            }//end for loop for printing Instruction log


            filewrite.WriteLine();
            filewrite.WriteLine("______________________________________________________________________________________________");
            filewrite.WriteLine();
            filewrite.WriteLine("Cycle count: " + cycleCount);
            filewrite.WriteLine("Hazard count: " + hazardCount);


            filewrite.Flush();
            filewrite.Close();
            label8.Text = "Saved";


        }

        /*/ single cycle processes, going to overwrite old ones

        public void SimAgain()
        {
            while (this.SimulationCount < this.Input_Instructions.Count)
            {
                CountUpdate();
                UpdateAndDelay();


                CycleRegister();
                CycleMemory();
                CycleExecute();
                CycleDecode();
                CycleFetch();

                UpdateAndDelay();
            }

            // clean up pipeline

            while (this.Fetch.Count != 0 || this.Decode.Count != 0 || this.Execute.Count != 0 || this.Memory.Count != 0 || this.Register.Count != 0)
            {
                CountUpdate();
                UpdateAndDelay();

                CycleRegister();

                if (this.Fetch.Count == 0 && this.Decode.Count == 0 && this.Execute.Count == 0 && this.Memory.Count == 0)
                    return;

                CycleMemory();
                CycleExecute();
                CycleDecode();

            }
        }

        public void CycleFetch()
        {
            Instruction temp;
            if (this.Fetch.Count > 0)
            {
                temp = this.Fetch.Pop();
                this.Decode.Push(temp);
                DecodeText(temp);
            }
            else if (this.SimulationCount < this.Input_Instructions.Count && this.Fetch.Count == 0)
            {
                PushFetch(this.Input_Instructions[this.SimulationCount]);
                this.SimulationCount++;
            }
        }

        public void CycleDecode()
        {
            Instruction temp;
            if (this.Decode.Count == 0 && this.Fetch.Count > 0)     // decode for one cycle
            {
                temp = this.Fetch.Pop();
                PushDecode(temp);
                UpdateAndDelay();
            }
        }

        public void CycleExecute()
        {
            Instruction temp;
            if (this.Execute.Count > 0)     // Execute for one cycle
            {
                temp = this.Execute.Peek();

                if (temp.ExecuteCC == 0)
                {
                    if (temp.ExecuteCC == 0 && temp.MemoryCC == 0 && temp.RegisterCC == 0)
                    {
                        this.Execute.Pop();
                    }
                    else if (temp.MemoryCC > 0 && this.Memory.Count == 0)
                    {
                        this.Execute.Pop();
                        PushMemory(temp);
                        MemoryText(temp);
                    }
                    else if (temp.RegisterCC > 0 && this.Register.Count == 0)
                    {
                        this.Execute.Pop();
                        this.ExecuteBox.Text = "";
                        PushRegister(temp);
                        RegisterText(temp);
                    }
                    //else if (temp.ExecuteCC > 0)
                    //  temp.ExecuteCC--;
                }

            }
            else if (this.Execute.Count == 0 && this.Decode.Count > 0)
            {
                temp = this.Decode.Pop();
                this.Execute.Push(temp);
                ExecuteText(temp);
            }


        }

        public void CycleMemory()
        {
            Instruction temp;
            if (this.Memory.Count > 0)      // memory for one cycle
            {
                temp = this.Memory.Peek();

                if (temp.MemoryCC == 0)
                {
                    temp = this.Memory.Pop();
                    this.MemoryBox.Text = "";

                    if (temp.RegisterCC > 0)
                    {
                        PushRegister(temp);
                    }
                }
                else if (temp.MemoryCC > 0)
                {
                    temp.MemoryCC--;
                }
                UpdateAndDelay();
            }
            else if (this.Memory.Count == 0 && this.Execute.Count > 0)
            {
                temp = this.Execute.Pop();
                this.Memory.Push(temp);
                MemoryText(temp);
            }
        }

        public void CycleRegister()
        {
            if (this.Register.Count > 0)    // register for one cycle
            {
                this.Register.Pop();
                this.RegisterBox.Text = "";
                UpdateAndDelay();
            }
            else if ()

        } */
    }
}
