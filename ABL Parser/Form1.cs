using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace ABL_Parser
{
    public partial class Form1 : Form
    {
        int indent = 0;
        bool ifline = false;
        int closeifnextline = -1;
        public Form1()
        {
            InitializeComponent();
        }

        private string spaceindent()
        {
            string spaces = "";
            if (ifline) { for (int f = 0; f < indent-1; f++) { spaces = spaces + "  "; } ifline = false; } //don't want if line itself indented
                   else { for (int f = 0; f < indent; f++) { spaces = spaces + "  "; } }
            return spaces;
        }
        private string endlinesemicolon(string line)
        {
            if (line.Length > 0 && line.Substring(line.Length-1, 1) == ".")
            {
                line = line.Substring(0, line.Length - 1) + ";";
            }
            return line;
        }

        private string definevariable(string line)
        {
            string var = "";
            line = line.Substring(16, line.Length - 16);
            var = line.Substring(0, line.IndexOf(" "));
            if (line.ToLower().IndexOf("as char") > 0) { line = "string " + var + " = \"\";"; }
            if (line.ToLower().IndexOf("as integer") > 0) { line = "int " + var + " = 0;"; }
            if (line.ToLower().IndexOf("as decimal") > 0) { line = "decimal " + var + " = 0m;"; }
            if (line.ToLower().IndexOf("as logical") > 0) { line = "bool " + var + " = false;"; }
            return line;
        }
        private string fieldlabel(string line)
        {
            try
            {
                string var = "";
                string beginline = line.Substring(0, line.IndexOf("fieldlabel"));
                string endline = line.Substring(line.IndexOf("fieldlabel") + 10);
                var = endline.Substring(endline.IndexOf("'") + 1, endline.Length - endline.IndexOf("'") - 1);
                var = var.Substring(0, var.IndexOf("'"));
                var = var.Replace("Value", "Label = ");
                endline = endline.Substring(endline.IndexOf(",") + 1);
                endline = endline.Replace(")", "");
                endline = endline.Replace(".", ";");
                line = beginline + var + endline;
                
            }
            catch
            {
                line = "ERROR : " + line;
            }

            return line;

        }
        private string fieldinvisible(string line)
        {
            try
            {
                string var = "";
                var = line.Substring(line.IndexOf("Inputs"));
                var = var.Substring(0, var.IndexOf("'"));
                var = var.Replace("Value", "Invisible = ");
                line = line.Substring(line.IndexOf(",") + 1).ToLower();
                if (line.IndexOf("true") > 0) { line = var + "true;"; }
                if (line.IndexOf("false") > 0) { line = var + "false;"; }
            }
            catch
            {
                line = "ERROR : " + line;
            }

            return line;
        }
        private string showmessage(string line)
        {
            try
            {
                if (line.IndexOf("'") > 0)
                {
                    line = line.Substring(line.IndexOf("'") + 1, line.Length - line.IndexOf("'") - 1);
                    line = line.Substring(0, line.IndexOf("'"));
                }
                else
                {
                    line = line.Substring(line.IndexOf("&ExMsg") + 6);
                    line = line.Substring(line.IndexOf("=") + 1);
                    if (line.Substring(0, 1) == " ") { line = line.Substring(1); }
                    line = line.Substring(0, line.IndexOf("}"));
                }
                return "MessageBox.Show(" + line + ");";
            }
            catch
            {
                line = "Error :" + line;
            } 
            return line;

        }
        private string setnextinputpage(string line)
        {
            string page = line.Substring(line.ToLower().IndexOf("setnextinputpage") + 16);
            page = page.Substring(page.IndexOf("(") + 1);
            return "PageLeaveFunctions.SetNextInputPage(" + page.Substring(0,page.IndexOf(")")) + ");";
        }
        private string filelookup(string line)
        {
            try
            {
                string output = line.Substring(line.IndexOf("output") + 7, line.IndexOf(" ", line.IndexOf("output") + 7) - line.IndexOf("output") - 7);
                line = line.Substring(line.IndexOf("("));
                string lookupTable = line.Substring(line.IndexOf("\"") + 1, line.IndexOf("\"", line.IndexOf("\"") + 1) - 2);
                try
                {
                    lookupTable = "\"" + Path.GetFileNameWithoutExtension(lookupTable) + "\"";
                }
                catch
                {
                    lookupTable = "\"" + "Error" + "\"";
                }
                line = line.Substring(line.IndexOf(",") + 1);
                string columnName = line.Substring(0, line.IndexOf(","));
                string rowName = line.Substring(line.IndexOf(",") + 1);
                rowName = rowName.Substring(0, rowName.IndexOf(","));
                return output + " = PCLookUp.DataLookup(" + lookupTable + ", " + columnName + ", " + rowName + ");";
            }
            catch(System.ArgumentException ex)
            {
                Debug.Print(ex.Message);
            }
            return line;

        }
        private string findfirst(string line)
        {
            try

            {
                line = line.Substring(line.ToLower().IndexOf("find first") + 11);
                string item = line.Substring(0, line.IndexOf(" "));
                string inputs = "";
                if (line.IndexOf("Inputs") > 0)
                {
                    inputs = line.Substring(line.IndexOf("Inputs"));
                    inputs = inputs.Substring(0, inputs.IndexOf(" "));
                }
                else
                {
                    while (line.IndexOf("=") > 0)
                    {
                        line = line.Substring(line.IndexOf("=") + 1).Trim();
                    }
                    inputs = line.Substring(0, line.IndexOf(" "));
                }
                return "available_" + item + " = UDMethods.Get" + item + "Function(Context.CompanyID," + inputs + ", out " + item + "item);";
            }
            catch
            {
            }

            return "ERROR : " + line;

             


        }
        private string endstatement(string line)
        {
            indent--;
            return "}";
        }
        private string TrUeFaLsE(string line)
        {
            line = line.Replace("== FALSE", "== false");
            line = line.Replace("== False", "== false");
            line = line.Replace("== TRUE", "== true");
            line = line.Replace("== True", "== true");
            return line;
        }
        private string strings(string line)
        {
            line = line.Replace("String(Mode)", "Mode");
            line = line.Replace("String(QuoteNum)", "QuoteNum.ToString()");
            line = line.Replace("String(OrderNum)", "OrderNum.ToString()");
            line = line.Replace("String(CustName)", "CustName");
            line = line.Replace("String(Compweight1)", "CompWeight1.ToString()");
            line = line.Replace("String(Compweight2)", "CompWeight2.ToString()");
            line = line.Replace("String(Compweight3)", "CompWeight3.ToString()");
            line = line.Replace("String(Compweight4)", "CompWeight4.ToString()");
            line = line.Replace("String(Compweight5)", "CompWeight5.ToString()");
            line = line.Replace("String(Compweight6)", "CompWeight6.ToString()");
            line = line.Replace("String(Compweight7)", "CompWeight7.ToString()");
            line = line.Replace("String(Compweight8)", "CompWeight8.ToString()");
            line = line.Replace("String(Compweight9)", "CompWeight9.ToString()");
            line = line.Replace("String(Compweight10)", "CompWeight10.ToString()");
            line = line.Replace("String(Compweight11)", "CompWeight11.ToString()");
            line = line.Replace("String(Compweight12)", "CompWeight12.ToString()");
            line = line.Replace("String(Compweight13)", "CompWeight13.ToString()");
            line = line.Replace("String(Compweight14)", "CompWeight14.ToString()");
            line = line.Replace("String(Compweight15)", "CompWeight15.ToString()");
            line = line.Replace("String(Compweight16)", "CompWeight16.ToString()");
            line = line.Replace("String(Compweight17)", "CompWeight17.ToString()");
            line = line.Replace("String(Compweight18)", "CompWeight18.ToString()");
            line = line.Replace("String(Compweight19)", "CompWeight19.ToString()");
            line = line.Replace("String(Compweight20)", "CompWeight20.ToString()");
            return line;
        }
        private string entry(string line)
        {
            try
            {
                string s_entry = line.Substring(line.ToLower().IndexOf("entry(") + 6);
                string entrynum = s_entry.Substring(0, s_entry.IndexOf(","));
                entrynum = (Convert.ToInt32(entrynum) - 1).ToString();
                string var = s_entry.Substring(s_entry.IndexOf(",") + 1);
                var = var.Substring(0, var.IndexOf(","));
                s_entry = s_entry.Substring(s_entry.IndexOf(",") + 1);
                s_entry = s_entry.Substring(s_entry.IndexOf(",") + 1);
                string entrychar = s_entry.Substring(0, s_entry.IndexOf(")"));
                line = line.Substring(0, line.ToLower().IndexOf("entry("));
                line = line + var + ".Entry(" + entrynum + "," + s_entry;
                return line;
            }
            catch
            {
                return "ERROR : " + line;
            }

        }
        private string ifstatement(string line)
        {
            ifline = true;
            string newline = "";
            string statements = "";
            string statement = "";
            int orpos = 0;
            int andpos = 0;
            int andorpos = 0;
            string andor = "";
            int andorsize = 0;
            closeifnextline = -1;
            if (line.ToLower().IndexOf("then do") > 0) { indent++; }
            if ((line.ToLower().IndexOf(" or ") > 0) || (line.ToLower().IndexOf(" and ") > 0)) { newline = "if ("; } else { newline = "if "; }

            statements = line.Substring(3, line.ToLower().IndexOf("then") - 4);
            while ((statements.ToLower().IndexOf(" or ") > 0) || (statements.ToLower().IndexOf(" and ") > 0)) {
                orpos = statements.ToLower().IndexOf(" or ");
                andpos = statements.ToLower().IndexOf(" and ");
                if (orpos > 0 && andpos < 0 ) { andorpos = orpos; andor = " || "; andorsize = 3; } //or but no and
                if (orpos < 0 && andpos > 0) { andorpos = andpos; andor = " && "; andorsize = 4; } //and but no or
                if (orpos > 0 && andpos > 0 && orpos < andpos) {andorpos = orpos-2; andor = " || "; andorsize = 3; }
                if (orpos > 0 && andpos > 0 && orpos > andpos) { andorpos = andpos - 3; andor = " && "; andorsize = 4; }
                statement = statements.Substring(0, andorpos);
                statement = statement.Replace("=", "==");
                statement = statement.Replace("<>", "!=");
                newline = newline + "(" + statement + ")" + andor;
                statements = statements.Substring(andorpos+andorsize, statements.Length - andorpos - andorsize);
            }
            statements = statements.Replace("=", "==");
            statements = statements.Replace("<>", "!=");
            newline = newline + "(" + statements + ")";

            if (newline.Substring(0,5) == "if ((") { newline = newline + ")"; }
            if (line.ToLower().IndexOf("then do") > 0) { return newline + " {"; }
            else if (line.ToLower().IndexOf("then ") > 0) { return newline + " {" + line.Substring(line.ToLower().IndexOf("then ") + 5) + "}"; }
            else { closeifnextline = 1; return newline + " {";  }
        }
        private string ParseLine(string line)
        {
            line = line.Replace("// ", "");

            if (line.Length > 15 && line.ToLower().Substring(0,15) == "define variable") { line = definevariable(line); }
            if (line.Length > 2 && line.ToLower().Substring(0, 2) == "if") { line = ifstatement(line); }
            if (line.Length > 7 && line.ToLower().Substring(0, 7) == "else if") { line = "else " + ifstatement(line.Substring(8,line.Length - 8)); }
            if (line.Length >= 4 && line.ToLower().Substring(0, 4) == "end.") { line = endstatement(line); }
            if (line.ToLower().IndexOf("publishex.i") > 0) { line = showmessage(line); }
            if (line.ToLower().IndexOf("setnextinputpage") > 0) { line = setnextinputpage(line); }
            if (line.ToLower().IndexOf("filelookup.p") > 0) { line = filelookup(line); }
            if (line.ToLower().IndexOf("find first") >= 0) { line = findfirst(line); }
            if (line.ToLower().IndexOf("fieldlabel") >= 0) { line = fieldlabel(line); }
            if (line.ToLower().IndexOf("fieldinvisible") >= 0) { line = fieldinvisible(line); }
            if (line.ToLower().IndexOf("entry(") >= 0) { line = entry(line); }
            line = line.Replace("available ", "bool available_");
            line = line.Replace("GetContextMode()", "Context.Entity");
            line = line.Replace("GetCurrentOrderNum()", "Context.OrderNumber");
            line = line.Replace("decimal(", "UDMethods.ConvertToDecimal(");
            line = line.Replace("GetCurrentQuoteNum()", "Context.QuoteNumber");
            line = strings(line);
            line = endlinesemicolon(line);
            if (closeifnextline == 0) { line = line + "}"; closeifnextline--; }
            if (closeifnextline > 0) { closeifnextline--;}
            if (line.Length > 2 && line.Substring(line.Length - 2) == ".}") { line = line.Replace(".}", ";}"); }
            line = TrUeFaLsE(line);
            return spaceindent() + line;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < textBox1.Lines.Count(); i++)
            {
                textBox2.AppendText(ParseLine(textBox1.Lines[i]) + Environment.NewLine);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Show();
        }
    }
}
