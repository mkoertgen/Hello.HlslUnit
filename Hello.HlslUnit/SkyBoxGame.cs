using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using SharpDX.Toolkit.Input;

namespace Hello.HlslUnit
{
    public class SkyBoxGame : Game
    {
        // ReSharper disable once NotAccessedField.Local
        private GraphicsDeviceManager _deviceManager;

        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        private readonly StringBuilder _text = new StringBuilder();

        private Matrix _view;

        private readonly KeyboardManager _keyboard;
        private KeyboardState _keyboardState;
        private readonly MouseManager _mouse;
        private MouseState _mouseState, _oldMouseState;

        private readonly Vector3 _originalView = new Vector3(0, 0, 10);
        private float _angleX;
        private float _angleY;

        private GeometricPrimitive _skyBox;
        private Effect _skyBoxEffect;
        private readonly List<TextureCube> _skyBoxMaps = new List<TextureCube>();
        private readonly String[] _skyBoxNames =
        {
            "StarCube", "SunnyDayCube"
        };
        private int _currentSkyBox;
        private TextureCube _skyBoxMap;
        private Matrix _skyProjection;

        public SkyBoxGame()
        {
            _deviceManager = new GraphicsDeviceManager(this)
            {
#if DEBUG
                DeviceCreationFlags = DeviceCreationFlags.Debug,
#endif
                DepthBufferShaderResource = true,
                PreferredDepthStencilFormat = DepthFormat.None,
                PreferredGraphicsProfile = new[] { FeatureLevel.Level_11_0, FeatureLevel.Level_10_1, FeatureLevel.Level_10_0 }
            };

            Content.RootDirectory = "Content";
            _keyboard = new KeyboardManager(this);
            _mouse = new MouseManager(this);
        }

        protected override void Initialize()
        {
            Window.Title = "SkyBoxGame";
            Window.AllowUserResizing = true;
            IsMouseVisible = true;

            base.Initialize();

            _keyboardState = _keyboard.GetState();
            _mouseState = _mouse.GetState();
        }

        protected override void LoadContent()
        {
            _spriteBatch = ToDisposeContent(new SpriteBatch(GraphicsDevice));
            _font = Content.Load<SpriteFont>("Arial16");
            _view = Matrix.LookAtLH(Vector3.Zero, _originalView, Vector3.Up);

            _skyBoxEffect = Content.Load<Effect>("SkyBoxEffect");
            _skyBoxMaps.Clear();
            _skyBoxMaps.AddRange(_skyBoxNames.Select(name => Content.Load<TextureCube>(name)));
            _skyBoxMap = _skyBoxMaps[_currentSkyBox];

            _skyBox = ToDisposeContent(GeometricPrimitive.Cube.New(GraphicsDevice));
            //_skyBox = ToDisposeContent(GeometricPrimitive.Sphere.New(GraphicsDevice));

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            UpdateInput();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            DrawSky();
            DrawText();
            base.Draw(gameTime);
        }

        private void UpdateInput()
        {
            _keyboardState = _keyboard.GetState();

            _oldMouseState = _mouseState;
            _mouseState = _mouse.GetState();

            if (KeyPressed(Keys.Escape)) Exit();

            if (KeyPressed(Keys.Space))
            {
                _currentSkyBox = (_currentSkyBox + 1) % _skyBoxMaps.Count;
                _skyBoxMap = _skyBoxMaps[_currentSkyBox];
            }

            if (KeyPressed(Keys.B))
            {
                _skyBox.Dispose();
                _skyBox = ToDisposeContent(GeometricPrimitive.Cube.New(GraphicsDevice));
            }
            if (KeyPressed(Keys.D))
            {
                _skyBox.Dispose();
                _skyBox = ToDisposeContent(GeometricPrimitive.Sphere.New(GraphicsDevice));
            }

            if (KeyDown(Keys.Home)) _angleX = _angleY = 0.0f;
            if (KeyDown(Keys.Up)) _angleX -= 0.01f;
            if (KeyDown(Keys.Down)) _angleX += 0.01f;
            if (KeyDown(Keys.Left)) _angleY -= 0.01f;
            if (KeyDown(Keys.Right)) _angleY += 0.01f;

            if (_mouseState.LeftButton.Down && _oldMouseState.LeftButton.Down)
            {
                _angleY += _mouseState.X - _oldMouseState.X;
                _angleX += _mouseState.Y - _oldMouseState.Y;
            }

            UpdateView();
        }

        private bool KeyPressed(Keys key)
        {
            return _keyboardState.IsKeyPressed(key);
        }

        private bool KeyDown(Keys key)
        {
            return _keyboardState.IsKeyDown(key);
        }

        private void UpdateView()
        {
            if (_angleX < -MathUtil.PiOverTwo) _angleX = -MathUtil.PiOverTwo;
            else if (_angleX > MathUtil.PiOverTwo) _angleX = MathUtil.PiOverTwo;

            var tempView = Vector3.TransformCoordinate(_originalView, Matrix.RotationX(_angleX));
            tempView = Vector3.TransformCoordinate(tempView, Matrix.RotationY(_angleY));
            var tempUp = Vector3.TransformCoordinate(Vector3.Up, Matrix.RotationX(_angleX));
            tempUp = Vector3.TransformCoordinate(tempUp, Matrix.RotationY(_angleY));
            _view = Matrix.LookAtLH(Vector3.Zero, tempView, tempUp);
        }

        private void DrawSky()
        {
            _skyProjection = Matrix.PerspectiveFovLH(MathUtil.PiOverFour, GraphicsDevice.Viewport.AspectRatio, 0.01f, 1.0f);
            _skyBoxEffect.Parameters["ViewProj"].SetValue(_view * _skyProjection);
            _skyBoxEffect.Parameters["CubeMap"].SetResource(_skyBoxMap);
            _skyBox.Draw(_skyBoxEffect);
        }

        private void DrawText()
        {
            _text.Clear();
            _text.AppendLine("Keys:");
            _text.AppendLine("  Home  - Home Position");
            _text.AppendLine("  Space - Toggle Skybox Maps");
            _text.AppendLine("  B/D   - SkyBox / SkyDome");
            _text.AppendLine();
            _text.AppendFormat("{5} \"{0}\" ({1}/{2}): {3}x{3} {4}",
                _skyBoxNames[_currentSkyBox], _currentSkyBox + 1, _skyBoxNames.Length,
                _skyBoxMap.Width, _skyBoxMap.IsBlockCompressed ? "DXT" : "Uncompressed",
                _skyBox.Name);

            using (_spriteBatch.Block())
            {
                _spriteBatch.DrawString(_font, _text.ToString(), new Vector2(16, 16), Color.White);
            }
        }
    }
}
