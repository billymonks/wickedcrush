using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() //this is the sloppiest mess ever there is no way this is ever going on a portfolio of any kind
        { // at least it works, but you must keep this secret with your life
            Form1 sheetCreatorForm = new Form1(); // if someone finds this just pretend you stole it. swear on your life that someone must have hacked your computer and remotely uploaded it to the project directory
            Cheater cheat = new Cheater(); //i just want the content manager and graphics device is that so much to ask for
            cheat.Run(); // derp derp derp
            sheetCreatorForm._content = cheat.Content; //herp
            sheetCreatorForm._graphics = cheat.GraphicsDevice; //herp
            Application.Run(sheetCreatorForm);
        }
    }
}
