﻿using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using OpenTK.Graphics;
using KoC.GameEngine.Draw;
using System.Runtime;
using System.Collections.Generic;
using System.Threading;
using KoC.GameEngine.Draw.Renderer;
using KoC.GameEngine.Draw.Text;
using System.Drawing;
using KoC.GameEngine.ShaderManager;

namespace KoC.GameEngine
{
	public sealed class Game : GameWindow
	{
		private bool powerLimiter;

		//Constructor
		public Game() : base(
				500,
				500,
				GraphicsMode.Default,
				"The Game Project",
				GameWindowFlags.Default,
				DisplayDevice.Default,
				4,
				5,
				GraphicsContextFlags.ForwardCompatible
			)
		{
			Title += ": OpenGL Version: " + GL.GetString(StringName.Version);
			powerLimiter = true;

			KeyDown += InputHandler.onKeyDown;
			KeyUp += InputHandler.onKeyUp;

			EngineText2D[] text = new EngineText2D[1];
			text[0] = new EngineText2D(new KoCFont(new Font("Consolas", 64f)), "AEBCD", new Vector2(0f, 0f));
			
			StaticHolder.textureHandler = new TextureHandler();
			StaticHolder.textHandler = new TextHandler(text);
			StaticHolder.loader = new Loader.GameLoader();
		}
		/// <summary>
		/// Switches Power Saving Mode if on can cause stutter.<br/>
		/// If off users with bad drivers can have performance issues
		/// 
		/// </summary>
		public void SwitchPowerLimiter()
		{
			powerLimiter = !powerLimiter;
		}
		public bool IsPowerSaved()
		{ 
			return powerLimiter; 
		}
		public void SwitchVSync(VSyncMode vsync)
		{
			VSync = vsync;
		}

		//Overrides
		#region Overrides
		protected override void OnLoad(EventArgs e)
		{
			CursorVisible = true;
			Shader[] shaders = new Shader[2];
			shaders[0] = new Shader(@"Shaders\verShader.vert", ShaderType.VertexShader);
			shaders[1] = new Shader(@"Shaders\fragShader.frag", ShaderType.FragmentShader);

			ShaderProgram _program = new ShaderProgram(shaders,"main");

			GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

			GL.PatchParameter(PatchParameterInt.PatchVertices, 3);
			
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.Blend);

			GL.BlendFunc(BlendingFactor.SrcAlpha,BlendingFactor.OneMinusSrcAlpha);
			GL.DepthFunc(DepthFunction.Less);

			Closed += OnClosed;

			Mesh[] m = new Mesh[2];
			GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
			GC.Collect();


			List<Obj3D> Obj3DList = new List<Obj3D>();
			for(int i = 0; i < m.Length; i++)
			{
				Obj3DList.Add(
					new Obj3D(
						ref m[i],
						new Vector3(0f,0f,-10f),
						string.Empty
						)
					);
			}

			Vector3 CameraPos		= new Vector3(1.0f, 1.0f, -10.0f);
			Vector3 CameraTarget	= new Vector3(0.45f, 0.0f, 1.0f);

			StaticHolder.mainRender = new RenderManager(Obj3DList, new Player.Camera(CameraTarget,CameraPos),_program);
			StaticHolder.mainRender.ReloadProjections(Width,Height);

			StaticHolder.isRenderFreezed = true;
			while(!StaticHolder.loader.AreAllFilesReady())
			{
				Thread.Sleep(300);
			}
			StaticHolder.isRenderFreezed = false;
			
			base.OnLoad(e);
		}
		private void OnClosed(object s, EventArgs e)
		{
			Exit();
		}
		public override void Exit()
		{
			StaticHolder.mainRender.Delete();
			StaticHolder.textureHandler.Delete();
			base.Exit();
		}
		protected override void OnResize(EventArgs e)
		{
			float asp;
			GL.Viewport(ClientRectangle);
			asp = StaticHolder.GetRatio(Width,Height);
			StaticHolder.mainRender.ReloadProjections(asp);
			base.OnResize(e);
		}
		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			base.OnUpdateFrame(e);
		}
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			if (!StaticHolder.isRenderFreezed)
			{
				if (powerLimiter) Thread.Sleep(15);
				Color4 backColor = new Color4(0, 0, 80, 255);
				GL.ClearColor(backColor);
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

				Title = $"(Vsync : {VSync}) (FPS : {1f / e.Time:0})";

				StaticHolder.mainRender.RenderCall();

				SwapBuffers();
			}
			//Error Catch
			#if DEBUG
				StaticHolder.CheckGLError();
			#endif
		}
		#endregion Overrides
	}
}
