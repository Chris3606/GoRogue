using GoRogue;
using System;
using System.Diagnostics;
using System.Linq;

namespace GoRogue_PerformanceTests
{
	internal class MyEffect : Effect<EffectArgs>
	{
		public MyEffect(int startingDuration)
			: base("MyEffect", startingDuration)
		{ }

		public int CurValue { get; private set; }

		

		protected override void OnTrigger(EffectArgs e)
		{
			CurValue++;
		}
	}
	internal static class EffectsTests
	{
		private static uint _thingToIncrementOnExpire = 0;

		public static TimeSpan TestEffectManagerNoExpiredHandler(int numEffects, int iterations)
		{
			var s = new Stopwatch();

			
			// For caching
			var myEffectManager = new EffectTrigger<EffectArgs>();
			foreach (int i in Enumerable.Range(0, numEffects))
				myEffectManager.Add(new MyEffect(iterations + 1));

			myEffectManager.TriggerEffects(new EffectArgs());

			s.Start();
			for (int i = 0; i < iterations; i++)
				myEffectManager.TriggerEffects(new EffectArgs());
			s.Stop();

			return s.Elapsed;
		}

		public static TimeSpan TestEffectManagerWithExpiredHandler(int numEffects, int iterations)
		{
			_thingToIncrementOnExpire = 0;
			var s = new Stopwatch();


			// For caching
			var myEffectManager = new EffectTrigger<EffectArgs>();
			foreach (int i in Enumerable.Range(0, numEffects))
			{
				var effect = new MyEffect(iterations + 1);
				effect.Expired += OnEffectExpired;
				myEffectManager.Add(effect);
			}

			myEffectManager.TriggerEffects(new EffectArgs());

			s.Start();
			for (int i = 0; i < iterations; i++)
				myEffectManager.TriggerEffects(new EffectArgs());
			s.Stop();

			if (_thingToIncrementOnExpire != numEffects)
				throw new Exception($"Performance test did not perform proper function, and cannot be considered valid.  Value was supposed to be {numEffects}, but was {_thingToIncrementOnExpire}");

			return s.Elapsed;
		}

		private static void OnEffectExpired(object s, EventArgs e) => _thingToIncrementOnExpire++;
	}
}
