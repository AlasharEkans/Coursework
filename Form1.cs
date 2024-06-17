using System;
using System.Drawing;
using System.Windows.Forms;

namespace koorsach
{
    public partial class Form1 : Form
    {
        private GameLogic game;

        public Form1()
        {
            InitializeComponent();
            this.ClientSize = new Size(GameLogic.BoardCols * GameLogic.CellSize, GameLogic.BoardRows * GameLogic.CellSize);
            this.Text = "Fox and Chickens Game";
            this.DoubleBuffered = true;
            this.Paint += new PaintEventHandler(this.Form1_Paint);
            this.MouseClick += new MouseEventHandler(this.Form1_MouseClick);

            game = new GameLogic(this);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            game.DrawBoard(g);
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            game.HandleMouseClick(e.X, e.Y);
        }
    }
}
