using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Nez
{
	/// <summary>
	/// A virtual input represented as a float between -1 and 1
	/// </summary>
	public class VirtualAxis : VirtualInput
	{
		public List<Node> Nodes = new List<Node>();

		public float Value
		{
			get
			{
				for (var i = 0; i < Nodes.Count; i++)
				{
					var val = Nodes[i].Value;
					if (val != 0)
						return val;
				}

				return 0;
			}
		}


		public VirtualAxis()
		{
		}


		public VirtualAxis(params Node[] nodes)
		{
			Nodes.AddRange(nodes);
		}


		public override void Update(GameTime time)
		{
			for (var i = 0; i < Nodes.Count; i++)
				Nodes[i].Update();
		}


		public static implicit operator float(VirtualAxis axis)
		{
			return axis.Value;
		}


		#region Node types

		public abstract class Node : VirtualInputNode
		{
			public abstract float Value { get; }
		}

		public class KeyboardKeys : Node
		{
			public OverlapBehavior OverlapBehavior;
			public Keys Positive;
			public Keys Negative;

			float _value;
			bool _turned;


			public KeyboardKeys(OverlapBehavior overlapBehavior, Keys negative, Keys positive)
			{
				OverlapBehavior = overlapBehavior;
				Negative = negative;
				Positive = positive;
			}


			public override void Update()
			{
				if (Input.IsKeyDown(Positive))
				{
					if (Input.IsKeyDown(Negative))
					{
						switch (OverlapBehavior)
						{
							default:
							case OverlapBehavior.CancelOut:
								_value = 0;
								break;

							case OverlapBehavior.TakeNewer:
								if (!_turned)
								{
									_value *= -1;
									_turned = true;
								}

								break;
							case OverlapBehavior.TakeOlder:
								//value stays the same
								break;
						}
					}
					else
					{
						_turned = false;
						_value = 1;
					}
				}
				else if (Input.IsKeyDown(Negative))
				{
					_turned = false;
					_value = -1;
				}
				else
				{
					_turned = false;
					_value = 0;
				}
			}


			public override float Value => _value;
		}

		#endregion
	}
}