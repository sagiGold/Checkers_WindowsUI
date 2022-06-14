using Checkers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CheckersWindowsUI
{
    public delegate void chosenMoveEventHandler(Move i_Move);

    public partial class FormGame : Form
    {
        const int k_LengthOfSquare = 60;    // should be 80
        public event chosenMoveEventHandler chosenMove;
        public event Action NewMatch;
        private PictureBox[,] m_SquaresBoard;
        private PictureBox r_ToolChosen = null;
        eTeamSign m_CurrentTeamTurn;

        public eTeamSign CurrentTeamTurn
        {
            set 
            {
                m_CurrentTeamTurn = value;
            }
        }

        public FormGame()
        {
            InitializeComponent();
        }

        public void UpdateScoreLabels(string i_PlayerOneName, int i_PlayerOneScore, string i_PlayerTwoName, int i_PlayerTwoScore)
        {
            this.labelPlayer1Score.Text = string.Format("{0} : {1}", i_PlayerOneName, i_PlayerOneScore);
            this.labelPlayer2Score.Text = string.Format("{0} : {1}", i_PlayerTwoName, i_PlayerTwoScore);
        }

        public void initButtons(int i_BoardSize)
        {
            m_SquaresBoard = new PictureBox[i_BoardSize, i_BoardSize];

            for (int row = 0; row < i_BoardSize; row++)
            {
                for (int column = 0; column < i_BoardSize; column++)
                {
                    m_SquaresBoard[row, column] = new PictureBox
                    {
                        Size = new System.Drawing.Size(k_LengthOfSquare, k_LengthOfSquare),
                        Location = new System.Drawing.Point(column * k_LengthOfSquare, row * k_LengthOfSquare),
                        BackColor = Color.LightGreen // for test
                    };
                    m_SquaresBoard[row, column].Click += pictureBox_Clicked;
                    splitContainer1.Panel2.Controls.Add(m_SquaresBoard[row, column]);
                }      
            }
        }

        public void initMatrixToBoard(Board i_Board)
        {
            for (int i = 0; i < i_Board.Size; ++i)
            {
                for (int j = 0; j < i_Board.Size; ++j)
                {
                    if (i % 2 == 0 && j % 2 == 0)
                    {
                        m_SquaresBoard[i, j].Enabled = false;
                    }
                    else if (i_Board[i, j] != null)
                    {
                        m_SquaresBoard[i, j].BackColor = i_Board[i, j].TeamSign == eTeamSign.PlayerX ?
                           Color.White : Color.CadetBlue;      // need to put a picture for team instead of color
                        m_SquaresBoard[i, j].Tag = i_Board[i, j].TeamSign;
                    }
                    else
                    {
                        m_SquaresBoard[i, j].BackColor = Color.LightGreen;
                        m_SquaresBoard[i, j].Tag = null;
                    }
                }
            }
        }

        public void UnavailableMove()
        {
            MessageBox.Show("Unavailable Move, please choose new move");
        }

        public void game_GameOver(string i_WinnerName)
        {
            DialogResult result = MessageBox.Show(string.Format("{0} won!{1}Another Round?",
                                    i_WinnerName, Environment.NewLine), "Damka", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                NewMatch?.Invoke();
            }
            else
            {
                Close();
            }
        }

        private void pictureBox_Clicked(object sender, EventArgs e)
        {
            PictureBox chosenPictureBox = sender as PictureBox;

            if (r_ToolChosen == null && chosenPictureBox.Tag is eTeamSign sign && sign.Equals(m_CurrentTeamTurn))
            {
                r_ToolChosen = chosenPictureBox;
                chosenPictureBox.BackColor = Color.LightBlue;
            }
            else if(r_ToolChosen != null)
            {
                if (r_ToolChosen == chosenPictureBox)
                {
                    chosenPictureBox.BackColor = Color.LightGreen;
                    r_ToolChosen = null;
                }
                else
                {
                    OnChosenMove(chosenPictureBox);  
                }
            }
        }

        protected virtual void OnChosenMove(PictureBox i_DestinationBox)
        {
            Checkers.Point fromLocation = new Checkers.Point(r_ToolChosen.Location.X / k_LengthOfSquare, r_ToolChosen.Location.Y / k_LengthOfSquare);
            Checkers.Point toLocation = new Checkers.Point(i_DestinationBox.Location.X / k_LengthOfSquare, i_DestinationBox.Location.Y / k_LengthOfSquare);
            Move nextMove = initNextMove(fromLocation, toLocation);

            //toolChosen.BackColor = Color.Transparent;
            r_ToolChosen = null;
            if (chosenMove != null)
            {
                chosenMove(nextMove);
            }
        }

        private Move initNextMove(Checkers.Point i_FromLocation, Checkers.Point i_ToLocation)
        {
            Move nextMove = new Move(i_FromLocation, i_ToLocation);

            nextMove.MoveTool += nextMove_MoveTool;
            nextMove.EatTool += nextMove_EatTool;
            nextMove.SwitchedToKing += nextMove_switchKing;

            return nextMove;
        }

        private void nextMove_switchKing(Checkers.Point i_Location)
        {
            // switch images for the box in loacation point
        }

        private void nextMove_EatTool(Checkers.Point i_Location)
        {
            //squaresBoard[i_Location.Y, i_Location.X].Image = null;
            m_SquaresBoard[i_Location.Y, i_Location.X].BackColor = Color.LightGreen;
            m_SquaresBoard[i_Location.Y, i_Location.X].Tag = null;
        }

        private void nextMove_MoveTool(Checkers.Point i_From, Checkers.Point i_Destination)
        {
            /*Image tempImage = squaresBoard[i_From.Y, i_From.X].Image;

            squaresBoard[i_From.Y, i_From.X].Image = squaresBoard[i_Destination.Y, i_Destination.X].Image;
            squaresBoard[i_Destination.Y, i_Destination.X].Image = tempImage;
        */
            Color tempImage = m_SquaresBoard[i_From.Y, i_From.X].BackColor;

            m_SquaresBoard[i_From.Y, i_From.X].BackColor = m_SquaresBoard[i_Destination.Y, i_Destination.X].BackColor;
            m_SquaresBoard[i_Destination.Y, i_Destination.X].BackColor= tempImage;

            eTeamSign tempSign = (eTeamSign)m_SquaresBoard[i_From.Y, i_From.X].Tag;

            m_SquaresBoard[i_From.Y, i_From.X].Tag = m_SquaresBoard[i_Destination.Y, i_Destination.X].Tag;
            m_SquaresBoard[i_Destination.Y, i_Destination.X].Tag= tempSign;
        }
    }
}
