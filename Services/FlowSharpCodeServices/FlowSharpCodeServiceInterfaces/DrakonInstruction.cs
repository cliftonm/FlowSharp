/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Collections.Generic;

namespace FlowSharpCodeServiceInterfaces
{
    public interface ICodeGeneratorService
    {
        void BeginIf(string code);
        void Else();
        void EndIf();
        void BeginFor(string code);
        void EndFor();
        void Statement(string code);
    }

    public abstract class DrakonInstruction
    {
        public string Code { get; set; }
        public abstract void GenerateCode(ICodeGeneratorService codeGenSvc);
    }

    public class DrakonCodeGenerator
    {
        public bool HasInstructions { get { return instructions.Count > 0; } }
        protected List<DrakonInstruction> instructions;

        public DrakonCodeGenerator()
        {
            instructions = new List<DrakonInstruction>();
        }

        public void AddInstruction(DrakonInstruction instruction)
        {
            instructions.Add(instruction);
        }

        public void GenerateCode(ICodeGeneratorService codeGenSvc)
        {
            instructions.ForEach(inst => inst.GenerateCode(codeGenSvc));
        }
    }

    public class DrakonIf : DrakonInstruction
    {
        public DrakonCodeGenerator TrueInstructions { get; protected set; }
        public DrakonCodeGenerator FalseInstructions { get; protected set; }

        public DrakonIf()
        {
            TrueInstructions = new DrakonCodeGenerator();
            FalseInstructions = new DrakonCodeGenerator();
        }

        public override void GenerateCode(ICodeGeneratorService codeGenSvc)
        {
            codeGenSvc.BeginIf(Code);
            TrueInstructions.GenerateCode(codeGenSvc);

            if (FalseInstructions.HasInstructions)
            {
                codeGenSvc.Else();
                FalseInstructions.GenerateCode(codeGenSvc);
            }

            codeGenSvc.EndIf();
        }
    }

    //public class DrakonEndIf : DrakonInstruction
    //{
    //    public override void GenerateCode(ICodeGeneratorService codeGenSvc)
    //    {
    //        codeGenSvc.EndIf();
    //    }
    //}

    public class DrakonBeginLoop : DrakonInstruction
    {
        public override void GenerateCode(ICodeGeneratorService codeGenSvc)
        {
            codeGenSvc.BeginFor(Code);
        }
    }

    public class DrakonEndLoop : DrakonInstruction
    {
        public override void GenerateCode(ICodeGeneratorService codeGenSvc)
        {
            codeGenSvc.EndFor();
        }
    }

    public class DrakonOutput : DrakonInstruction
    {
        public override void GenerateCode(ICodeGeneratorService codeGenSvc)
        {
            codeGenSvc.Statement(Code);
        }
    }

    public class DrakonStatement : DrakonInstruction
    {
        public override void GenerateCode(ICodeGeneratorService codeGenSvc)
        {
            codeGenSvc.Statement(Code);
        }
    }
}
