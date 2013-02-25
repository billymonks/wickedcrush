using System;

namespace Raditor
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            EditorForm form = new EditorForm();
            form.Show();
            Editor game = new Editor(form.getDrawSurface());
            form.getContentManager(game.Content); // this is probably terrible and i'm going to hell
            form.getGraphicsDevice(game.GraphicsDevice);
            form.getEditor(game);
            game.Run();
            
        }
    }
#endif
}

