using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Logic
{
	public class WaveRun
	{
		private readonly Map map;
		private readonly Vector startPosition;

		public WaveRun(Map map, Vector startPosition)
		{
			this.map = map;
			this.startPosition = startPosition;
		}

		public Tuple<Vector, RobotMove[]> Lift { get; private set; }

		public IEnumerable<Tuple<Vector, RobotMove[]>> EnumerateTargets()
		{
			var q = new Queue<WaveCell>();
			q.Enqueue(new WaveCell(startPosition, 0, null, RobotMove.Wait));
			var used = new HashSet<Vector>();
			while (q.Any())
			{
				var cell = q.Dequeue();
				if (map[cell.Pos] == MapCell.Lambda) yield return CreateTarget(cell);
				if (map[cell.Pos] == MapCell.OpenedLift) Lift = CreateTarget(cell);
				foreach (var move in new[]{RobotMove.Down, RobotMove.Left, RobotMove.Right, RobotMove.Up, })
				{
					var newPos = cell.Pos.Add(move.ToVector());
					if (!used.Contains(newPos) && map.IsValidMoveWithoutMovingRocks(cell.Pos, newPos) && map.IsSafeMove(cell.Pos, newPos))
						q.Enqueue(new WaveCell(newPos, cell.StepNumber + 1, cell, move));
					used.Add(newPos);
				}
			}
		}

		private Tuple<Vector, RobotMove[]> CreateTarget(WaveCell targetCell)
		{
			IList<RobotMove> moves = new List<RobotMove>();
			var cell = targetCell;
			while (cell.PrevCell != null)
			{
				moves.Add(cell.Move);
				cell = cell.PrevCell;
			}
			return Tuple.Create(targetCell.Pos, moves.Reverse().ToArray());
		}

		private class WaveCell
		{
			public WaveCell(Vector pos, int stepNumber, WaveCell prevCell, RobotMove move)
			{
				Pos = pos;
				StepNumber = stepNumber;
				PrevCell = prevCell;
				Move = move;
			}

			public readonly Vector Pos;
			public readonly int StepNumber;
			public readonly RobotMove Move;
			public readonly WaveCell PrevCell;

		}
	}

	[TestFixture]
	public class WaveRun_Test
	{
		[Test]
		public void Test()
		{
			Map map = WellKnownMaps.Contest1();
			var formattedTargets = GetTargets(map.Robot, map);
			Assert.That(formattedTargets, Contains.Item("(4, 4) via DL"));
			Assert.That(formattedTargets, Contains.Item("(2, 3) via DLLDL"));
			
			formattedTargets = GetTargets(new Vector(2, 2), map);
			Assert.That(formattedTargets, Contains.Item("(2, 3) via U"));
		}

		private static string[] GetTargets(Vector from, Map map)
		{
			Console.WriteLine(map.ToString());
			var waveRun = new WaveRun(map, from);
			Tuple<Vector, RobotMove[]>[] targets = waveRun.EnumerateTargets().ToArray();
			string[] formattedTargets = targets.Select(FormatTarget).ToArray();
			foreach (var target in formattedTargets)
				Console.WriteLine(target);
			return formattedTargets;
		}

		private static string FormatTarget(Tuple<Vector, RobotMove[]> target)
		{
			var commands = new String(target.Item2.Select(m => m.ToChar()).ToArray());
			string format = target.Item1 + " via " + commands;
			return format;
		}
	}
}