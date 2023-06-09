﻿using System;
using System.IO;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Template
{
	public class OpenTKApp : GameWindow
	{
		static int screenID;
        static int debugID;
		static Game game;
		static bool terminated = false;
		protected override void OnLoad( EventArgs e )
		{
			// called upon app init
			GL.ClearColor( Color.Black );
			GL.Enable( EnableCap.Texture2D );
			GL.Disable( EnableCap.DepthTest );
			GL.Hint( HintTarget.PerspectiveCorrectionHint, HintMode.Nicest );
			ClientSize = new Size( 1024, 512 );
			game = new Game();
			game.screen = new FloatSurface( Width/2, Height );
            game.debug = new Surface(Width / 2 ,Height);
			Sprite.target = game.debug;
			screenID = game.screen.GenTexture();
            debugID = game.debug.GenTexture();
			game.Init();
		}
		protected override void OnUnload( EventArgs e )
		{
			// called upon app close
			GL.DeleteTextures( 1, ref screenID );
            GL.DeleteTextures( 1, ref debugID );
			Environment.Exit( 0 ); // bypass wait for key on CTRL-F5
		}
		protected override void OnResize( EventArgs e )
		{
			// called upon window resize
			GL.Viewport(0, 0, Width, Height);
			GL.MatrixMode( MatrixMode.Projection );
			GL.LoadIdentity();
			GL.Ortho( -1.0, 1.0, -1.0, 1.0, 0.0, 4.0 );
		}
		protected override void OnUpdateFrame( FrameEventArgs e )
		{
			// called once per frame; app logic
			var keyboard = OpenTK.Input.Keyboard.GetState();
			if (keyboard[OpenTK.Input.Key.Escape]) this.Exit();
		}
		protected override void OnRenderFrame( FrameEventArgs e )
		{
			// called once per frame; render
			game.Tick();
			if (terminated) 
			{
				Exit();
				return;
			}
			// clear window contents
			GL.Clear( ClearBufferMask.ColorBufferBit );
			// setup camera
			GL.MatrixMode( MatrixMode.Modelview );
			GL.LoadIdentity();
			GL.MatrixMode( MatrixMode.Projection );
			GL.LoadIdentity();

            // convert Game.screen to OpenGL texture
            GL.BindTexture( TextureTarget.Texture2D, screenID );
            GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb32f, 
                           game.screen.width, game.screen.height, 0, 
                          OpenTK.Graphics.OpenGL.PixelFormat.Rgb, 
                          PixelType.Float, game.screen.pixels 
                         );
			// draw screen filling quad
			GL.Begin( PrimitiveType.Quads );
			GL.TexCoord2( 0.0f, 1.0f ); GL.Vertex2( -1.0f, -1.0f );
			GL.TexCoord2( 1.0f, 1.0f ); GL.Vertex2(  0.0f, -1.0f );
			GL.TexCoord2( 1.0f, 0.0f ); GL.Vertex2(  0.0f,  1.0f );
			GL.TexCoord2( 0.0f, 0.0f ); GL.Vertex2( -1.0f,  1.0f );
			GL.End();

            // convert Game.debug to OpenGL texture
            GL.BindTexture( TextureTarget.Texture2D, debugID );
            GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 
                           game.debug.width, game.debug.height, 0, 
                          OpenTK.Graphics.OpenGL.PixelFormat.Bgra, 
                          PixelType.UnsignedByte, game.debug.pixels 
                         );
            // draw screen filling quad
            GL.Begin( PrimitiveType.Quads );
            GL.TexCoord2( 0.0f, 1.0f ); GL.Vertex2(  0.0f, -1.0f );
            GL.TexCoord2( 1.0f, 1.0f ); GL.Vertex2(  1.0f, -1.0f );
            GL.TexCoord2( 1.0f, 0.0f ); GL.Vertex2(  1.0f,  1.0f );
            GL.TexCoord2( 0.0f, 0.0f ); GL.Vertex2(  0.0f,  1.0f );
            GL.End();

			// tell OpenTK we're done rendering
			SwapBuffers();
		}
		public static void Main( string[] args ) 
		{ 
			// entry point
			using (OpenTKApp app = new OpenTKApp()) { app.Run( 30.0, 0.0 ); }
		}
	}
}