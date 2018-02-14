using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game_Jam
{
    enum GameState
    {
        InGame,
        Menu,
        Pause
    }

    internal class GameData
    {
         public Player player;
        
        private float dt;
        
        public int CurrentRoom = 0;
        
        public Rect bounds;
        public GameData(float z, Vector2 playerStartPos, Rect Bounds)
        {
            player = default(Player);
            player.Init(z, playerStartPos);
            bounds = Bounds;
        }

        public void Update(float deltatime)
        {
            dt = deltatime;
            Vector2 PlayerPrevPos = player.Pos;
            player.Update(dt);
            if (player.GetDrawRectangle().min.Y < bounds.min.Y)
            {
                player.Pos.Y = PlayerPrevPos.Y;
                player.OnGround = true;
                player.Jumping = false;
            }
            if (player.GetDrawRectangle().min.X < bounds.min.X)
            {
                player.Pos.X = PlayerPrevPos.X;
            }
        }


    }
    public enum TextureID
    {
        font,
        pixel,
        background,
        player_left,
        player_right,
        MAX_TEXTURES
    }
    public struct Player
    {

        public Vector2 Pos;
        public Vector2 dPos;

        public Vector2 ddPos;

        public Vector2 Dim;

        public float Zindex;

        public FacingDirection Direction;

        public TextureID tex_id;

        public float drag;

        public float acc;

        public bool OnGround;
        public bool Jumping;
        public void Init(float z, Vector2 pos)
        {
            Dim = new Vector2(16f, 23f);
            tex_id = TextureID.player_right;
            Direction = FacingDirection.Right;
            Pos = pos;
            Zindex = z;
            OnGround = false;
            Jumping = false;
            acc = 200f;
            drag = 0.009f;
        }
        
        public Rect GetDrawRectangle()
        {
            return new Rect(Pos - Dim / 2f, Dim);
        }
        
        public void MoveLeft()
        {
            Direction = FacingDirection.Left;
            ddPos -= Vector2.UnitX;
        }
        
        public void MoveTo()
        {
        }
        
        public void Jump(float height)
        {
            if (OnGround && !Jumping)
            {
                ddPos.Y += height;
                OnGround = false;
                Jumping = true;
            }
        }
        public void MoveRight()
        {
            Direction = FacingDirection.Right;
            ddPos += Vector2.UnitX;
        }
        
        public void Update(float dt)
        {
            bool gravity = true;
            if (gravity)
            {
                ddPos.Y -= 10f;
            }
            Vector2 OlddPos = dPos;
            dPos += ddPos * acc * dt;
            Pos += (dPos + OlddPos) * 0.5f * dt;
            ddPos = Vector2.Zero;
            dPos *= 1f - drag;
            switch (Direction)
            {
                case FacingDirection.Right:
                    tex_id = TextureID.player_right;
                    break;
                case FacingDirection.Left:
                    tex_id = TextureID.player_left;
                    break;
            }
        }
 }
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;

        private SpriteBatch spriteBatch;

        private Vector2 RealScreenDim;

        private Vector2 ScreenDim;

        private Texture2D pixel;

        private SpriteFont font;

        private List<DrawCommand> DrawList;

        private List<Texture2D> Textures_2D;

        private MouseState MouseState;

        private KeyboardState KeyboardState;

        private MouseState PrevMouseState;

        private KeyboardState PrevKeyboardState;

        private List<Label> Labels = new List<Label>();

        private bool GameInit = false;

        private GameData gameData;

        private float timer;

        private bool starttextanim = true;

        private Vector2 movetopoint = Vector2.Zero;

        private bool setmovepoint;
        float updatetimer;
        GameState State = GameState.Menu;
        public struct Label
        {
            public Label(string strs, OnMouseClick del = null)
            {
                str = strs;
                OnClick = del;
            }

            public string str;

            public OnMouseClick OnClick;

            public delegate void OnMouseClick();
        }

        public struct Button
        {
            public Button(string strs, OnMouseClick del = null)
            {
                str = strs;
                OnClick = del;
            }

            public string str;

            public OnMouseClick OnClick;

            public delegate void OnMouseClick();
        }

        private void LoadTexture(string str)
        {
            Textures_2D.Add(Content.Load<Texture2D>(str));
        }

        private Texture2D GetPixelTexture(Color col)
        {
            Texture2D tex = new Texture2D(GraphicsDevice, 1, 1);
            tex.SetData<Color>(new Color[]
            {
                col
            });
            return tex;
        }

        public Game1()
        {
            IsMouseVisible = true;
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 800,
                PreferredBackBufferHeight = 600
            };
            Content.RootDirectory = "Content";
            IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        private void InitializeTextures()
        {
            Textures_2D = new List<Texture2D>();
            for (int i = 0; i < 5; i++)
            {
                TextureID t = (TextureID)i;
                if (t == TextureID.pixel)
                {
                    Textures_2D.Add(GetPixelTexture(Color.White));
                }
                else
                {
                    if (t == TextureID.font)
                    {
                        font = Content.Load<SpriteFont>("Font");
                        Textures_2D.Add(font.Texture);
                    }
                    else
                    {
                        LoadTexture(t.ToString());
                    }
                }
            }
        }

        protected override void LoadContent()
        {
            RealScreenDim = new Vector2((float)GraphicsDevice.Viewport.Width, (float)GraphicsDevice.Viewport.Height);
            ScreenDim = new Vector2(1920f, 1080f) * 2f;
            DrawList = new List<DrawCommand>();
            spriteBatch = new SpriteBatch(GraphicsDevice);
            pixel = GetPixelTexture(Color.White);
            InitializeTextures();
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }
            PrevMouseState = MouseState;
            PrevKeyboardState = KeyboardState;
            KeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();
            Vector2 MousePos = new Vector2((float)MouseState.X, (float)(GraphicsDevice.Viewport.Height - MouseState.Y)) / RealScreenDim * ScreenDim;
            Vector2 PrevMousePos = new Vector2((float)PrevMouseState.X, (float)(GraphicsDevice.Viewport.Height - PrevMouseState.Y)) / RealScreenDim * ScreenDim;
            Vector2 MousedPos = MousePos - PrevMousePos;
            float heightfactor = 0.6f;
            int pad = 30;

            float textduration = 2f;
            Rect bottomview = new Rect(0f, 0f, ScreenDim.X, ScreenDim.Y * heightfactor);
            if (!GameInit)
            {
                gameData = new GameData(2f, new Vector2(300f, 150f), new Rect(Vector2.Zero, bottomview.Dim));
                gameData.player.Dim = Textures_2D[(int) gameData.player.tex_id].Bounds.Size.ToVector2() * 3f;
                gameData.player.acc = 50;
                GameInit = true;
                Labels.Add(new Label("Hello"));
                Labels.Add(new Label(" "));
                Labels.Add(new Label("World"));
                Labels.Add(new Label("NL"));
                Labels.Add(new Label("Embarking on an adventure!!!!"));
                Labels.Add(new Label("NL"));
                    
            }
            
            switch (State)
            {
                case GameState.InGame:
                    {
                        List<Label> TempLabels = new List<Label>();
                        Rect border = bottomview;
                        border.max.X = border.max.X - border.Dim.X * 0.9f;
                        border = bottomview;
                        border.min.X = border.min.X + border.Dim.X * 0.9f;
                        border = bottomview;
                        border.max.Y = border.max.Y - border.Dim.Y * 0.9f;
                        border = bottomview;
                        border.min.Y = border.min.Y + border.Dim.X * 0.9f;
                        Rect prevbottomview = bottomview;
                        bottomview.Inflate((float)(-(float)pad));
                        if (KeyboardState.IsKeyDown(Keys.Left))
                        {
                            int room = gameData.CurrentRoom - 1 < 0 ? 0 : gameData.CurrentRoom - 1;
                            if (!setmovepoint) movetopoint = gameData.bounds.Dim * (room) * Vector2.UnitX;
                            setmovepoint = true;
                        }
                        if (KeyboardState.IsKeyDown(Keys.Right))
                        {
                            int room = gameData.CurrentRoom + 1 < 0 ? 0 : gameData.CurrentRoom + 1;
                            if (!setmovepoint) movetopoint = gameData.bounds.Dim * (room) * Vector2.UnitX;
                            setmovepoint = true;

                        }

                        if (PrevKeyboardState.IsKeyUp(Keys.Space) && KeyboardState.IsKeyDown(Keys.Space))
                        {
                            gameData.player.Jump(300f);
                        }
                        if (KeyboardState.IsKeyUp(Keys.Z) && PrevKeyboardState.IsKeyDown(Keys.Z))
                        {
                            starttextanim = !starttextanim;
                            timer = 0f;
                        }


                        if (gameData.player.Pos.X < movetopoint.X + gameData.player.Dim.X && gameData.player.Pos.X > movetopoint.X - gameData.player.Dim.X)
                        {
                            setmovepoint = false;
                        }
                        if (setmovepoint)
                        {
                            Vector2 target = movetopoint - gameData.player.Pos;
                            if (target.X < 0f)
                            {
                                gameData.player.MoveLeft();
                            }
                            else
                            {
                                gameData.player.MoveRight();
                            }
                        }

                        if (State == GameState.InGame)
                        {
                            updatetimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                            float updateTime = 1f / 60;

                            while (updatetimer >= updateTime)
                            {
                                gameData.Update(updateTime);
                                updatetimer -= updateTime;
                            }
                        }

                        float clearduration = 1f;
                        if (starttextanim)
                        {
                            if (timer < textduration)
                            {
                                timer += dt;
                            }
                            else
                            {
                                starttextanim = false;
                                timer = 0;
                            }
                        }

                        if (!starttextanim && timer >= clearduration)
                        {
                            timer = 0;
                            if (Labels.Count > 0)
                            {
                                Labels.RemoveAt(0);
                                while (Labels.Count > 0 && Labels[0].str != "NL")
                                {
                                    Labels.RemoveAt(0);
                                }
                                if (Labels.Count > 0) Labels.RemoveAt(0);
                            }
                        }
                        else
                        {
                            timer += dt;
                        }

                        if (!starttextanim && Labels.Count > 0)
                        {
                            if (Labels[0].str == "NL" || Labels[0].str == " ")
                            {
                                Labels.RemoveAt(0);
                            }
                        }
                        /// TOP VIEW

                        Rect playerdrawrect = gameData.player.GetDrawRectangle();
                        gameData.CurrentRoom = (int)(playerdrawrect.max.X / gameData.bounds.max.X);
                        playerdrawrect.min.X = playerdrawrect.min.X % gameData.bounds.max.X;
                        playerdrawrect.max.X = playerdrawrect.max.X % gameData.bounds.max.X;
                        playerdrawrect.min += bottomview.min;
                        float scrollfactor = playerdrawrect.min.X;

                        Rect Backgroundrect = bottomview;
                        Backgroundrect.max.X += gameData.bounds.max.X * 1.1f;
                        Backgroundrect.min.X += gameData.bounds.max.X * 0.9f;
                        Rect Backgroundrect2 = bottomview;
                        Backgroundrect.max.X = Backgroundrect.max.X - playerdrawrect.min.X;
                        Backgroundrect.min.X = Backgroundrect.min.X - playerdrawrect.min.X;
                        Backgroundrect2.max.X = Backgroundrect2.max.X - playerdrawrect.min.X;
                        Backgroundrect2.min.X = Backgroundrect2.min.X - playerdrawrect.min.X;
                        Backgroundrect.max.Y = prevbottomview.max.Y;
                        Backgroundrect2.max.Y = prevbottomview.max.Y;

                        DrawList.Add(new DrawCommand(gameData.player.tex_id, gameData.player.Zindex, playerdrawrect, Color.White));
                        DrawList.Add(new DrawCommand(TextureID.background, gameData.player.Zindex - 0.5f, Backgroundrect, Color.White));
                        DrawList.Add(new DrawCommand(TextureID.background, gameData.player.Zindex - 0.5f, Backgroundrect2, Color.White));
                        Rect topview = new Rect(0f, ScreenDim.Y * heightfactor, ScreenDim.X, ScreenDim.Y * (1f - heightfactor));
                        DrawList.Add(new DrawCommand(TextureID.pixel, 0f, topview, Color.BlueViolet));
                        topview.Inflate((float)(-(float)pad));
                        DrawList.Add(new DrawCommand(TextureID.background, 1f, topview, Color.White));


                        /// BOTTOM (TEXT) VIEW
                        Vector2 fontscale = new Vector2(3f);
                        Vector2 fontdim = font.MeasureString("A");
                        Vector2 startpad = new Vector2(20f, -20f);
                        Vector2 startpos = topview.TopLeft + startpad + new Vector2(0f, -fontdim.Y * fontscale.Y);
                        Vector2 textpos = startpos;
                        int movedistance = 400;
                        TempLabels.Add(new Label("Current Room : " + gameData.CurrentRoom));
                        TempLabels.Add(new Label("NL"));
                        TempLabels.Add(new Label("Player pos : " + gameData.player.Pos));
                        TempLabels.Add(new Label("NL"));
                        TempLabels.Add(new Label("Click to move to player right (" + movedistance + ")", delegate
                        {
                            movetopoint = gameData.player.Pos + new Vector2((float)movedistance, 0f);
                            setmovepoint = true;
                        }));
                        TempLabels.Add(new Label("NL"));
                        TempLabels.Add(new Label("Click to move to player left (" + movedistance + ")", delegate
                        {
                            movetopoint = gameData.player.Pos - new Vector2((float)movedistance, 0f);
                            setmovepoint = true;
                        }));
                        TempLabels.Add(new Label("NL"));
                        TempLabels.Add(new Label("JUMP", delegate
                        {
                            gameData.player.Jump(1000);
                        }));
                        TempLabels.Add(new Label("NL"));
                        TempLabels.Add(new Label("move to prev room ", delegate
                        {
                            int room = gameData.CurrentRoom - 1 < 0 ? 0 : gameData.CurrentRoom - 1;
                            if (!setmovepoint) movetopoint = gameData.bounds.Dim * (room) * Vector2.UnitX;
                            setmovepoint = true;
                        }));
                        TempLabels.Add(new Label("NL"));
                        TempLabels.Add(new Label("move to next room ", delegate
                        {

                            int room = gameData.CurrentRoom + 1 < 0 ? 0 : gameData.CurrentRoom + 1;
                            if (!setmovepoint) movetopoint = gameData.bounds.Dim * (room) * Vector2.UnitX;
                            setmovepoint = true;
                        }));
                        TempLabels.Add(new Label("Movetopos:" + movetopoint));
                        TempLabels.Add(new Label("NL"));
                        for (int i = 0; i < Labels.Count; i++)
                        {
                            Label lbl = Labels[i];
                            if (lbl.str == "NL")
                            {
                                textpos.Y -= fontdim.Y * fontscale.Y;
                                textpos.X = startpos.X;
                            }
                            else
                            {
                                int numchars = Math.Min((int)Math.Round((double)(timer / (textduration / (float)lbl.str.Length))), lbl.str.Length);
                                if (!starttextanim) numchars = lbl.str.Length;
                                string chars = lbl.str.Substring(0, numchars);
                                Rect strrect = new Rect(textpos, font.MeasureString(chars) * fontscale);
                                if (topview.Contains(strrect))
                                {
                                    DrawList.Add(new DrawCommand(chars, 1.5f, strrect, Color.White));
                                    if (strrect.Contains(MousePos))
                                    {
                                        if (chars != " ")
                                        {
                                            DrawList.Add(new DrawCommand(TextureID.pixel, 1.4999f, strrect, Color.Green));
                                        }
                                        if (MouseState.LeftButton == ButtonState.Released && PrevMouseState.LeftButton == ButtonState.Pressed)
                                        {
                                            lbl.OnClick?.Invoke();
                                        }
                                    }
                                }
                                textpos.X += strrect.Dim.X;
                            }
                        }
                        for (int i = 0; i < TempLabels.Count; i++)
                        {
                            Label lbl = TempLabels[i];
                            if (lbl.str == "NL")
                            {
                                textpos.Y -= fontdim.Y * fontscale.Y;
                                textpos.X = startpos.X;
                            }
                            else
                            {
                                int numchars = Math.Min((int)Math.Round((double)(timer / (textduration / (float)lbl.str.Length))), lbl.str.Length);
                                if (!starttextanim) numchars = lbl.str.Length;
                                string chars = lbl.str.Substring(0, numchars);
                                Rect strrect = new Rect(textpos, font.MeasureString(chars) * fontscale);
                                if (topview.Contains(strrect))
                                {
                                    DrawList.Add(new DrawCommand(chars, 1.5f, strrect, Color.White));
                                    if (strrect.Contains(MousePos))
                                    {
                                        if (chars != " ")
                                        {
                                            DrawList.Add(new DrawCommand(TextureID.pixel, 1.4999f, strrect, Color.Green));
                                        }
                                        if (MouseState.LeftButton == ButtonState.Released && PrevMouseState.LeftButton == ButtonState.Pressed)
                                        {
                                            lbl.OnClick?.Invoke();
                                        }
                                    }
                                }
                                textpos.X += strrect.Dim.X;
                            }
                        }

                    }
                    break;
                case GameState.Menu:
                    {
                        var PanelRect = new Rect(Vector2.Zero, ScreenDim);
                        PanelRect.Inflate(-PanelRect.Dim.X * 0.09f, -PanelRect.Dim.Y * 0.09f);

                        DrawList.Add(new DrawCommand(TextureID.pixel, 0, PanelRect, Color.Purple));
                        PanelRect.Inflate(-100);
                        DrawList.Add(new DrawCommand(TextureID.pixel, 0.5f, PanelRect, Color.BlueViolet));
                        List<Button> Buttons = new List<Button>();
                        Buttons.Add(new Button("Start",delegate
                        {
                            State = GameState.InGame;
                        }));
                        Buttons.Add(new Button("Quit",delegate
                        {
                            Exit();
                        }));
                        var PanelStart = PanelRect.TopLeft;
                        var buttondim = new Vector2(PanelRect.Dim.X, PanelRect.Dim.Y / Buttons.Count);
                        for (int i = 0; i < Buttons.Count; i++)
                        {
                            Color buttoncol = Color.DarkCyan;
                            PanelStart.Y -= buttondim.Y;
                            Button b = Buttons[i];
                            var buttonrect = new Rect(PanelStart, buttondim);
                            if (buttonrect.Contains(MousePos))
                            {
                                if (MouseState.LeftButton == ButtonState.Released
                                    && PrevMouseState.LeftButton == ButtonState.Pressed)
                                {
                                    b.OnClick?.Invoke();
                                }
                                buttoncol = Color.DarkMagenta;
                            }
                            DrawList.Add(new DrawCommand(TextureID.pixel, 1, buttonrect, buttoncol));
                            var strdim = font.MeasureString(b.str) * 10f;
                            DrawList.Add(new DrawCommand(b.str, 1.5f, new Rect(buttonrect.Center - (strdim / 2), strdim), Color.White));
                        }
                    }
                    break;
                case GameState.Pause:
                    {

                    }
                    break;
                default:
                    break;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Brown);
            DrawList.Sort(delegate (DrawCommand x, DrawCommand y)
            {
                int result;
                if (x.Zindex > y.Zindex)
                {
                    result = 1;
                }
                else
                {
                    if (y.Zindex > x.Zindex)
                    {
                        result = -1;
                    }
                    else
                    {
                        result = 0;
                    }
                }
                return result;
            });
            List<DrawCommand> SortedDrawList = (from x in DrawList
                                                group x by x.tex_id).SelectMany((IGrouping<TextureID, DrawCommand> x) => x.ToList<DrawCommand>()).ToList<DrawCommand>();
            SortedDrawList.Sort(delegate (DrawCommand x, DrawCommand y)
            {
                int result;
                if (x.Zindex > y.Zindex)
                {
                    result = 1;
                }
                else
                {
                    if (y.Zindex > x.Zindex)
                    {
                        result = -1;
                    }
                    else
                    {
                        result = 0;
                    }
                }
                return result;
            });
            Vector2 Scale = RealScreenDim / ScreenDim;
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
            for (int i = 0; i < SortedDrawList.Count; i++)
            {
                DrawCommand cmd = SortedDrawList[i];
                if (cmd.SourceRectangle == Rect.Zero)
                {
                    Rectangle src = Textures_2D[(int)cmd.tex_id].Bounds;
                    cmd.SourceRectangle = new Rect((float)src.Y, (float)src.X, (float)src.Width, (float)src.Height);
                }
                float minY = ScreenDim.Y - cmd.DestRectangle.min.Y;
                float maxY = ScreenDim.Y - cmd.DestRectangle.max.Y;
                if (maxY > minY)
                {
                    cmd.DestRectangle.min.Y = minY;
                    cmd.DestRectangle.max.Y = maxY;
                }
                else
                {
                    cmd.DestRectangle.min.Y = maxY;
                    cmd.DestRectangle.max.Y = minY;
                }
                cmd.DestRectangle *= Scale;
                cmd.DestRectangle.min.X = (float)Math.Round((double)cmd.DestRectangle.min.X);
                cmd.DestRectangle.max.X = (float)Math.Round((double)cmd.DestRectangle.max.X);
                cmd.DestRectangle.min.Y = (float)Math.Round((double)cmd.DestRectangle.min.Y);
                cmd.DestRectangle.max.Y = (float)Math.Round((double)cmd.DestRectangle.max.Y);
                Vector2 strscaledim = cmd.DestRectangle.Dim / font.MeasureString(cmd.str);
                float stringscale = Math.Min(strscaledim.X, strscaledim.Y);
                switch (cmd.Type)
                {
                    case DrawCommandType.DrawRect:

                        break;
                    case DrawCommandType.DrawString:
                        spriteBatch.DrawString(font, cmd.str, cmd.DestRectangle.BottomLeft, cmd.tint, 0f, Vector2.Zero, strscaledim, SpriteEffects.None, 0f);
                        break;
                    case DrawCommandType.DrawTexture:
                        spriteBatch.Draw(Textures_2D[(int)cmd.tex_id], cmd.DestRectangle.ToRectangle(), new Rectangle?(cmd.SourceRectangle.ToRectangle()), cmd.tint);
                        break;
                    default:
                        break;
                }
            }
            spriteBatch.End();
            DrawList.Clear();
            base.Draw(gameTime);
        }

      }
}
