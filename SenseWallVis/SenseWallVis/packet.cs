using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.RegularExpressions;

namespace SenseWallVis
{
    class packet
    {
        private float movementPercentage;
        private Node source;
        private Node destination;
        private SpriteBatch sb;
        private Texture2D pTexture;
        private string Label = "";
        SpriteFont myFnt;


        private Vector2 position;
        private float speed = 0.01f;

        public void setSpeed(float newSpeed)
        {
            speed = newSpeed;
        }

        public Node getSource()
        {
            return source;
        }

        public Node getDestination()
        {
            return destination;
        }

        public packet(SpriteBatch initSB, Texture2D initTexture, Node nodeA, Node nodeB)
        {
            source = nodeA;
            destination = nodeB;
            position = nodeA.getPosition();
            sb = initSB;
            pTexture = initTexture;
            Label = "";

        }

        public packet(SpriteBatch initSB, Texture2D initTexture, Node nodeA, Node nodeB,string newLabel, SpriteFont fnt)
        {
            source = nodeA;
            destination = nodeB;
            position = nodeA.getPosition();
            sb = initSB;
            pTexture = initTexture;
            Label = newLabel;
            myFnt = fnt;

        }

        public void draw()
        {

            //draw packet
            sb.Draw(pTexture, position-new Vector2(pTexture.Width/2.0f,pTexture.Height/2.0f), Color.White*0.7f);
            if (Label != "" && myFnt != null)
            {
                Label = CleanInput(Label);
                sb.DrawString(myFnt, Label, position - new Vector2(pTexture.Width / 2.0f, 0), Color.Black);
            }
        }

        static string CleanInput(string strIn)
        {
            // Replace invalid characters with empty strings.
            return Regex.Replace(strIn, @"[^\w\.@-]", "");
        }

        public float getMovementPercentage()
        {
            return movementPercentage;
        }

        public void update()
        {
            Vector2 cPos = new Vector2(0, 0);
            cPos.X = Math.Abs(source.getPosition().X - destination.getPosition().X);
            cPos.Y = Math.Abs(source.getPosition().Y - destination.getPosition().Y);

            if (source.getPosition().X < destination.getPosition().X)
            {
                cPos.X *= movementPercentage;
                cPos.X += source.getPosition().X;

            }
            else
            {
                cPos.X *= (1 - movementPercentage);
                cPos.X += destination.getPosition().X;

            }

            if (source.getPosition().Y < destination.getPosition().Y)
            {
                cPos.Y *= movementPercentage;
                cPos.Y += source.getPosition().Y;

            }
            else
            {
                cPos.Y *= (1 - movementPercentage);
                cPos.Y += destination.getPosition().Y;

            }

            movementPercentage += speed;

            position = cPos;

        }

        public Vector2 getPosition()
        {
            return position;            
        }
    }
}
