// ---------------------------------------------------------------------------
// File name:                   Instruction.cs
// Project name:                Project 2 - Harrison's Tangents
// ---------------------------------------------------------------------------
// Creator’s name:              Janine Day
// Edited By:                   Janine Day, Jason Middlebrook, 
// Course-Section:              CSCI-4717
// Creation Date:               02/17/2022
// ---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project2_HT
{
    /**
    * Class Name:       Instruction
    * Class Purpose:    Holds all information required for pipeline process (decoding and parsing through information)
    *
    * <hr>
    * Date created: 02/17/2022
    * @Janine Day
    */
    public class Instruction
    {
        public string Mnemonic;
        public uint OpCode;
        public string DestReg;
        public string Reg1;
        public string Reg2;
        public string Imm;
        public int FetchCC, DecodeCC, ExecuteCC, MemoryCC, RegisterCC;
        public bool writeBack;
        public bool useRD;
        public bool useR1;
        public bool useR2;
        public bool useImm;
        static List<Instruction> InstructionSet = new List<Instruction>()
        {
            new Instruction(0, "HALT", 1, 1, 1, 0, 0, false, false, false, false, false),
            new Instruction(1, "LOAD", 1, 1, 1, 3, 1, true, true, true, false, false),
            new Instruction(2, "STOR", 1, 1, 1, 3, 0, false, true, true, false, false),
            new Instruction(3, "ADD", 1, 1, 1, 0, 1, true, true, true, true, false),
            new Instruction(4, "ADDI", 1, 1, 1, 0, 1, true, true, true, false, true),
            new Instruction(5, "SUB", 1, 1, 1, 0, 1, true, true, true, true, false),
            new Instruction(6, "SUBI", 1, 1, 1, 0, 1, true, true, true, false, true),
            new Instruction(7, "BR", 1, 1, 1, 0, 0,  false, true, false, false, false),
            new Instruction(8, "BRLT", 1, 1, 1, 0, 0, false, true, false, false, false),
            new Instruction(9, "BRLE", 1, 1, 1, 0, 0, false, true, false, false, false),
            new Instruction(10, "BREQ", 1, 1, 1, 0, 0, false, true, false, false, false),
            new Instruction(11, "BRNE", 1, 1, 1, 0, 0, false, true, false, false, false),
            new Instruction(12, "BRGT", 1, 1, 1, 0, 0, false, true, false, false, false),
            new Instruction(13, "BRGE", 1, 1, 1, 0, 0, false, true, false, false, false),
            new Instruction(14, "AND", 1, 1, 1, 0, 1, true, true, true, true, false),
            new Instruction(15, "OR", 1, 1, 1, 0, 1,  true, true, true, true, false),
            new Instruction(16, "NOT", 1, 1, 1, 0, 1, true, true, false, false, false),
            new Instruction(17, "NEG", 1, 1, 1, 0, 1, true, true, false, false, false),
            new Instruction(18, "ASL", 1, 1, 1, 0, 1, true, true, true, true, false),               //R1 operand to be shifted, R2 shift value -JM
            new Instruction(19, "ASR", 1, 1, 1, 0, 1, true, true, true, true, false),
            new Instruction(20, "MOV", 1, 1, 1, 0, 1, true, true, true, false, false),
            new Instruction(128, "FADD", 1, 1, 2, 0, 1, true, true, true, true, false),
            new Instruction(129, "FSUB", 1, 1, 2, 0, 1, true, true, true, true, false),
            new Instruction(130, "FMULT", 1, 1, 5, 0, 1, true, true, true, true, false),
            new Instruction(131, "FDIV", 1, 1, 10, 0, 1, true, true, true, true, false),
            new Instruction(404, "INVALID", 1, 1, 0, 0, 0, false, false, false, false, false)
        };


        /**
        * Method Name: Instruction(int)
        * Method Purpose: Takes an instruction in hex and creates an instruction
        *
        * <hr>
        * Date created: 02/17/2022
        * @Janine Day
        * <hr>
        * @param int - hex input to covert into instruction
        */
        public Instruction(int instr)
        {
            Disassemble(instr);
        }

        /**
        * Method Name: Instruction(uint, string, int, int, int, int, int)
        * Method Purpose: Creates instructions given all factors, used to populate static InstructionSet
        *
        * <hr>
        * Date created: 03/04/2022
        * @Janine Day
        * <hr>
        * @param uint - opcode
        * @param string - mnemonic
        * @param int - fetch
        * @param int - decode
        * @param int - execute
        * @param int - memory
        * @param int - register
        */
        public Instruction(uint opcode, string mnemonic, int fetch, int decode, int execute, int memory, int register,bool writeBack, bool useRD, bool useR1, bool useR2, bool useImm)
        {
            this.OpCode = opcode;
            this.Mnemonic = mnemonic;
            this.FetchCC = fetch;
            this.DecodeCC = decode;
            this.ExecuteCC = execute;
            this.MemoryCC = memory;
            this.RegisterCC = register;

            if (this.RegisterCC == 1)
                this.writeBack = true;
            else
                this.writeBack = false;

            this.writeBack = writeBack;
            this.useRD = useRD;
            this.useR1 = useR1;
            this.useR2 = useR2;
            this.useImm = useImm;
            
        }

        /**
        * Method Name: FindIS()
        * Method Purpose: Goes through InstructionSet list to find an instruction or manually set INVALID
        *
        * <hr>
        * Date created: 03/04/2022
        * @Janine Day
        * <hr>
        */
        public void FindIS()
        {
            for (int i = 0; i < InstructionSet.Count; i++)
            {
                if (this.OpCode == InstructionSet[i].OpCode)
                {
                    this.Mnemonic = InstructionSet[i].Mnemonic;
                    this.FetchCC = InstructionSet[i].FetchCC;
                    this.DecodeCC = InstructionSet[i].DecodeCC;
                    this.ExecuteCC = InstructionSet[i].ExecuteCC;
                    this.MemoryCC = InstructionSet[i].MemoryCC;
                    this.RegisterCC = InstructionSet[i].RegisterCC;
                    this.useRD = InstructionSet[i].useRD;
                    this.useR1 = InstructionSet[i].useR1;
                    this.useR2 = InstructionSet[i].useR2;
                    this.useImm = InstructionSet[i].useImm;

                    return;
                }
            }

            this.OpCode = 404;
            this.Mnemonic = "INVALID";
            this.FetchCC = 1;
            this.DecodeCC = 1;
            this.ExecuteCC = 0;
            this.MemoryCC = 0;
            this.RegisterCC = 0;
            this.useRD = false;
            this.useR1 = false;
            this.useR2 = false;
            this.useImm = false;
        }



        /**
        * Method Name: disassemble(int)
        * Method Purpose: Takes the hex and separate each component into an instruction type global variable
        *
        * <hr>
        * Date created: 02/17/2022
        * @Janine Day
        * <hr>
        * @param int - hex input to covert into instruction
        */
        public void Disassemble(int input)
        {
            //separate the opcode
            this.OpCode = (uint)input & 0xFF000000;                 //must be unsigned so that r shift (>>) is logical, not arithmetic -JM
            this.OpCode >>= 24;

            this.Mnemonic = "";

            if (this.OpCode >= 128) //check if it's a floating point instruction-AM 
            {
                DisassembleFloat(input);
                return;
            }

            FindIS();
            // Sets Destination reg, reg 1, and reg 2 if the instruction uses the register -JM
            if (this.useRD == true)
            {
                uint rd = (uint)input & 0x00F00000;
                rd >>= 20;
                this.DestReg = "R" + rd.ToString("X");              // Uses shifts to isolate certain bits in instruction hex - JND
            }
            
            if (this.useR1 == true)
            {
                uint reg1 = (uint)input & 0x000F0000;
                reg1 >>= 16;
                this.Reg1 = "R" + reg1.ToString("X");
            }

            // Sets Destination reg, reg 1, and reg 2 if the instruction uses the register -JM
            if (this.useRD == true)
            {
                uint rd = (uint)input & 0x00F00000;
                rd >>= 20;
                this.DestReg = "R" + rd.ToString("X");              // Uses shifts to isolate certain bits in instruction hex - JND
            }
            
            if (this.useR1 == true)
            {
                uint reg1 = (uint)input & 0x000F0000;
                reg1 >>= 16;
                this.Reg1 = "R" + reg1.ToString("X");
            }

            if (this.useR2 == true)
            {
                uint reg2 = (uint)input & 0x0000F000;
                reg2 >>= 12;
                this.Reg2 = "R" + reg2.ToString("X");
            }

            if (this.useImm == true)                                //Put immediate value in Imm for ADDI and SUBI
            {
                uint immediate = (uint)input & 0x00000FFF;
                this.Imm = immediate.ToString("X");
            }

        }//end disassemble

       

        /// <summary>In the event that the instruction is a float, send it to its own method and then return to caller
        /// Avery Marlow</summary>
        /// <param name="input">FP instruction</param>
        public void DisassembleFloat(int input)
        {
            FindIS();
            //reuse code but slap an F on it - AM
            uint rd = (uint)input & 0x00F00000;
            rd >>= 20;
            this.DestReg = "FR" + rd.ToString("X");            // Uses shifts to isolate certain bits in instruction hex - JND
                                                               // Sets Destination reg, reg 1, and reg 2
            uint reg1 = (uint)input & 0x000F0000;
            reg1 >>= 16;
            this.Reg1 = "FR" + reg1.ToString("X");

            uint reg2 = (uint)input & 0x0000F000;
            reg2 >>= 12;
            this.Reg2 = "FR" + reg2.ToString("X");
        }

    }
}
