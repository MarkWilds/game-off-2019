using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace game.Input.Virtual
{
	/// <summary>
	/// A virtual input that is represented as a int that is either -1, 0, or 1. It corresponds to input that can range from on to nuetral to off
	/// such as GamePad DPad left/right. Can also use two keyboard Keys as the positive/negative checks.
	/// </summary>
	public class VirtualIntegerAxis : VirtualInput
	{
		public List<VirtualAxis.Node> Nodes = new List<VirtualAxis.Node>();

		public int Value
		{
			get
			{
				for (var i = 0; i < Nodes.Count; i++)
				{
					var val = Nodes[i].Value;
					if (val != 0)
						return Math.Sign(val);
				}

				return 0;
			}
		}


		public VirtualIntegerAxis()
		{
		}


		public VirtualIntegerAxis(params VirtualAxis.Node[] nodes)
		{
			Nodes.AddRange(nodes);
		}


		public override void Update(GameTime time)
		{
			for (var i = 0; i < Nodes.Count; i++)
				Nodes[i].Update();
		}


		#region Node management

		/// <summary>
		/// adds keyboard Keys to emulate left/right or up/down to this VirtualInput
		/// </summary>
		/// <returns>The keyboard keys.</returns>
		/// <param name="overlapBehavior">Overlap behavior.</param>
		/// <param name="negative">Negative.</param>
		/// <param name="positive">Positive.</param>
		public VirtualIntegerAxis AddKeyboardKeys(OverlapBehavior overlapBehavior, Keys negative, Keys positive)
		{
			Nodes.Add(new VirtualAxis.KeyboardKeys(overlapBehavior, negative, positive));
			return this;
		}

		#endregion


		public static implicit operator int(VirtualIntegerAxis axis)
		{
			return axis.Value;
		}
	}
}