﻿using System;

using WFS210;
using WFS210.Util;

namespace WFS210.Services
{
	public class SignalGenerator
	{
		/// <summary>
		/// Gets or sets the type of the signal.
		/// </summary>
		/// <value>The type of the signal.</value>
		public SignalType SignalType { get; set; }

		/// <summary>
		/// Gets or sets the frequency of the signal.
		/// </summary>
		/// <value>The frequency.</value>
		public double Frequency { get; set; }

		/// <summary>
		/// Gets or sets the amplitude of the signal.
		/// </summary>
		/// <value>The amplitude.</value>
		public double Amplitude { get; set; }

		/// <summary>
		/// Gets or sets the phase of the signal.
		/// </summary>
		/// <value>The phase.</value>
		public double Phase { get; set; }

		/// <summary>
		/// Gets or sets the offset of the signal.
		/// </summary>
		/// <value>The offset.</value>
		public double Offset { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="WFS210.Services.SignalGenerator"/> class.
		/// </summary>
		public SignalGenerator ()
		{
			this.SignalType = SignalType.Sine;
			this.Frequency = 100;
			this.Amplitude = 5;
			this.Phase = 0;
			this.Offset = 5;
		}

		/// <summary>
		/// Generates a signal for the specified channel.
		/// </summary>
		/// <param name="oscilloscope">Oscilloscope.</param>
		/// <param name="channelIndex">Index of the channel to generate to.</param>
		public void GenerateSignal (Oscilloscope oscilloscope, int channelIndex)
		{
			Channel channel = oscilloscope.Channels[channelIndex];

			Random random = new Random ();
			double randomPhase = random.Next (0, 628) / 100;

			for (int i = 0; i < channel.Samples.Count; i++) {

				double gnd = 1, input = 0;

				double tl = oscilloscope.Trigger.Level;

				switch (channel.InputCoupling) {
				case InputCoupling.AC:
					gnd = 1;
					input = 0;
					break;
				case InputCoupling.DC:
					gnd = 1;
					input = 1;
					break;
				case InputCoupling.GND:
					gnd = 0;
					input = 0;
					break;
				}

				double samplesPerVolt = 25 / VoltsPerDivisionConverter.ToVolts (channel.VoltsPerDivision);

				double a = Amplitude * samplesPerVolt;
				double o = input * Offset * samplesPerVolt;

				double samplesPerDiv;

				switch (oscilloscope.TimeBase) {
				case TimeBase.Tdiv1us:
					samplesPerDiv = 10;
					break;
				case TimeBase.Tdiv2us:
					samplesPerDiv = 20;
					break;
				default:
					samplesPerDiv = 50;
					break;
				}

				double t = i * TimeBaseConverter.ToSeconds (oscilloscope.TimeBase) / samplesPerDiv;

				if ((Math.Floor(a) == 0) || (Math.Floor(gnd) == 0)) {
					Phase = 0;
				} else if ((tl < (channel.YPosition - (o + a))) || (tl > (channel.YPosition + (o + a)))) {
					Phase = randomPhase;
				} else if (oscilloscope.Trigger.Slope == TriggerSlope.Rising) {
					Phase = Math.Asin ((tl - (channel.YPosition - o)) / a);
				} else {
					Phase = Math.PI - Math.Asin ((tl - (channel.YPosition - o)) / a);
				}

				int value = (int)Math.Round(channel.YPosition - gnd * (o + a * Math.Sin(2 * Math.PI * Frequency * t + Phase)));

				channel.Samples [i] = (byte)value.LimitToRange (0, 255);
			}

			AddNoise (channel.Samples);
		}

		public void AddNoise (SampleBuffer buffer)
		{
			Random random = new Random ();

			const int noiseLevel = 2;

			for (int i = 0; i < buffer.Count; i++) {

				buffer[i] += (byte)random.Next(-noiseLevel, noiseLevel);
			}
		}
	}
}

