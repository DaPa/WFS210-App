using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using WFS210;
using WFS210.IO;
using System.Timers;
using WFS210.Services;
using WFS210.Util;

namespace WFS210.UI
{
	partial class iWFS210ViewController : UIViewController
	{
		/// <summary>
		/// Wfs210 oscilloscope.
		/// </summary>
		protected readonly WFS210.Oscilloscope Oscilloscope;

		/// <summary>
		/// The service manager.
		/// </summary>
		protected readonly WFS210.Services.ServiceManager ServiceManager;

		/// <summary>
		/// Gets the service.
		/// </summary>
		/// <value>The service.</value>
		public Service Service {
			get { return ServiceManager.ActiveService; }
		}

		SettingsViewController settingsViewController;
		MeasurementsViewController measurementsViewController;

		SignalMeasurement[] signalMeasurements = new SignalMeasurement[2];
		MarkerMeasurement[] markerMeasurements = new MarkerMeasurement[2];

		/// <summary>
		/// Initializes a new instance of the <see cref="WFS210.UI.iWFS210ViewController"/> class.
		/// </summary>
		/// <param name="handle">Handle.</param>
		public iWFS210ViewController (IntPtr handle) : base (handle)
		{
			this.Oscilloscope = new Oscilloscope ();
			this.ServiceManager = new ServiceManager (Oscilloscope, ServiceType.Demo);
		}

		void SettingsChanged (object sender, EventArgs e)
		{
			UpdateScopeControls ();
			ScopeView.UpdateScopeView ();
		}

		#region View lifecycle

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			MainView.BackgroundColor = UIColor.FromPatternImage (UIImage.FromBundle ("BACKGROUND/BG-0x0.png"));
			Service.SettingsChanged += SettingsChanged;
			signalMeasurements [0] = new SignalMeasurement (){ Channel = 0, SelectedUnit = SignalUnit.Vdc };
			signalMeasurements [1] = new SignalMeasurement (){ Channel = 1,  SelectedUnit = SignalUnit.Vdc };
			markerMeasurements [0] = new MarkerMeasurement (){ Channel = 0,  SelectedUnit = MarkerUnit.dt };
			markerMeasurements [1] = new MarkerMeasurement (){ Channel = 1,  SelectedUnit = MarkerUnit.dt };
			ScopeView.Initialize (Oscilloscope);
			Timer timer = new Timer (200);
			timer.Elapsed += (object sender, ElapsedEventArgs e) => {
				Service.Update ();
				InvokeOnMainThread (ScopeView.UpdateScopeView);
				UpdateSignalMeasurement1();
				UpdateSignalMeasurement2();
			};
			timer.AutoReset = true;
			timer.Enabled = true;
			timer.Start ();

			ScopeView.SelectedChannel = Oscilloscope.Channels [0];

			ScopeView.NewData += (object sender, NewDataEventArgs e) => UpdateScopeControls ();

		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			UpdateScopeControls ();
		}

		public override bool PrefersStatusBarHidden ()
		{
			return true;
		}

		#endregion

		#region Events Channel1

		partial void btnSelectChannel1_TouchUpInside (UIButton sender)
		{
			ScopeView.SelectedChannel = Oscilloscope.Channels [0];
			UpdateScopeControls ();
		}

		partial void btnAC1_TouchUpInside (UIButton sender)
		{
			Service.Execute (new InputCouplingCommand (0, InputCoupling.AC));
		}

		partial void btnDC1_TouchUpInside (UIButton sender)
		{
			Service.Execute (new InputCouplingCommand (0, InputCoupling.DC));
		}

		partial void btnGND1_TouchUpInside (UIButton sender)
		{
			Service.Execute (new InputCouplingCommand (0, InputCoupling.GND));
		}

		partial void btnProbe1_TouchUpInside (UIButton sender)
		{
			Service.Execute (new FlipAttenuationFactorCommand (0));
		}

		partial void btnVoltDown1_TouchUpInside (UIButton sender)
		{
			Service.Execute (new NextVoltsPerDivisionCommand (0));
		}

		partial void btnVoltUp1_TouchUpInside (UIButton sender)
		{
			Service.Execute (new PreviousVoltsPerDivisionCommand (0));
		}

		partial void btnMarkerMeasurements_TouchUpInside (UIButton sender)
		{
			measurementsViewController = this.Storyboard.InstantiateViewController ("MeasurementsViewController") as MeasurementsViewController;
			measurementsViewController.ModalPresentationStyle = UIModalPresentationStyle.CurrentContext;
			measurementsViewController.ModalTransitionStyle = UIModalTransitionStyle.FlipHorizontal;
			MeasurementsViewController.isMarkerMeasurement = true;
			MeasurementsViewController.SelectedChannel = 0;
			MeasurementsViewController.SelectedMeasurement = markerMeasurements [1].SelectedUnit.ToString ("G");
			PresentViewController (measurementsViewController, true, null);
		}

		partial void btnSignalMeasurements_TouchUpInside (UIButton sender)
		{
			measurementsViewController = this.Storyboard.InstantiateViewController ("MeasurementsViewController") as MeasurementsViewController;
			// You need to specify the controller you are presenting 
			measurementsViewController.ModalPresentationStyle = UIModalPresentationStyle.CurrentContext;
			measurementsViewController.ModalTransitionStyle = UIModalTransitionStyle.FlipHorizontal;
			MeasurementsViewController.isMarkerMeasurement = false;
			MeasurementsViewController.SelectedChannel = 0;
			MeasurementsViewController.SelectedMeasurement = signalMeasurements [1].SelectedUnit.ToString ("G");
			PresentViewController (measurementsViewController, true, null);
		}

		#endregion

		#region Events Trigger

		partial void btnTriggerCH1_TouchUpInside (UIButton sender)
		{
			Service.Execute (new TriggerChannelCommand (0));
		}

		partial void btnTriggerCH2_TouchUpInside (UIButton sender)
		{
			Service.Execute (new TriggerChannelCommand (1));
		}

		partial void btnTriggerSlopeUp_TouchUpInside (UIButton sender)
		{
			Service.Execute (new TriggerSlopeCommand (TriggerSlope.Rising));
		}

		partial void btnTriggerSlopeDown_TouchUpInside (UIButton sender)
		{
			Service.Execute (new TriggerSlopeCommand (TriggerSlope.Falling)); 
		}

		partial void btnTriggerRun_TouchUpInside (UIButton sender)
		{
			Service.Execute (new TriggerModeCommand (TriggerMode.Run));
		}

		partial void btnTriggerNrml_TouchUpInside (UIButton sender)
		{
			Service.Execute (new TriggerModeCommand (TriggerMode.Normal));
		}

		partial void btnTriggerOnce_TouchUpInside (UIButton sender)
		{
			Service.Execute (new TriggerModeCommand (TriggerMode.Once));
		}

		partial void btnTriggerHold_TouchUpInside (UIButton sender)
		{
			Service.Execute (new ToggleHoldCommand ());
		}

		partial void btnTimeLeft_TouchUpInside (UIButton sender)
		{
			Service.Execute (new PreviousTimeBaseCommand ());
		}

		partial void btnTimeRight_TouchUpInside (UIButton sender)
		{
			Service.Execute (new NextTimeBaseCommand ());
		}

		partial void btnAutorange_TouchUpInside (UIButton sender)
		{
			Oscilloscope.AutoRange = !Oscilloscope.AutoRange;
			Service.ApplySettings ();  
		}

		#endregion

		#region Events Channel2

		partial void btnSelectChannel2_TouchUpInside (UIButton sender)
		{
			ScopeView.SelectedChannel = Oscilloscope.Channels [1];
			UpdateScopeControls ();
		}

		partial void btnAC2_TouchUpInside (UIButton sender)
		{
			Service.Execute (new InputCouplingCommand (1, InputCoupling.AC));
		}

		partial void btnDC2_TouchUpInside (UIButton sender)
		{
			Service.Execute (new InputCouplingCommand (1, InputCoupling.DC));
		}

		partial void btnGND2_TouchUpInside (UIButton sender)
		{
			Service.Execute (new InputCouplingCommand (1, InputCoupling.GND));
		}

		partial void btnProbe2_TouchUpInside (UIButton sender)
		{
			Service.Execute (new FlipAttenuationFactorCommand (1));
		}

		partial void btnVoltDown2_TouchUpInside (UIButton sender)
		{
			Service.Execute (new NextVoltsPerDivisionCommand (1));
		}

		partial void btnVoltUp2_TouchUpInside (UIButton sender)
		{
			Service.Execute (new PreviousVoltsPerDivisionCommand (1));
		}


		partial void btnMarkerMeasurements2_TouchUpInside (UIButton sender)
		{
			measurementsViewController = this.Storyboard.InstantiateViewController ("MeasurementsViewController") as MeasurementsViewController;
			measurementsViewController.ModalPresentationStyle = UIModalPresentationStyle.CurrentContext;
			measurementsViewController.ModalTransitionStyle = UIModalTransitionStyle.FlipHorizontal;
			MeasurementsViewController.isMarkerMeasurement = true;
			MeasurementsViewController.SelectedChannel = 1;
			MeasurementsViewController.SelectedMeasurement = markerMeasurements [1].SelectedUnit.ToString ("G");
			PresentViewController (measurementsViewController, true, null);
		}

		partial void btnSignalMeasurements2_TouchUpInside (UIButton sender)
		{
			measurementsViewController = this.Storyboard.InstantiateViewController ("MeasurementsViewController") as MeasurementsViewController;
			// You need to specify the controller you are presenting 
			measurementsViewController.ModalPresentationStyle = UIModalPresentationStyle.CurrentContext;
			measurementsViewController.ModalTransitionStyle = UIModalTransitionStyle.FlipHorizontal;
			MeasurementsViewController.isMarkerMeasurement = false;
			MeasurementsViewController.SelectedChannel = 1;
			MeasurementsViewController.SelectedMeasurement = signalMeasurements [1].SelectedUnit.ToString ("G");
			PresentViewController (measurementsViewController, true, null);
		}

		#endregion

		#region Events Settings

		partial void btnSettings_TouchUpInside (UIButton sender)
		{
			//throw new NotImplementedException();
			settingsViewController = this.Storyboard.InstantiateViewController ("SettingsViewController") as SettingsViewController;
			// You need to specify the controller you are presenting 
			settingsViewController.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;
			settingsViewController.ModalTransitionStyle = UIModalTransitionStyle.FlipHorizontal;
			PresentViewController (settingsViewController, true, null);
		}

		public void DismissSettingsViewController ()
		{

			settingsViewController.DismissViewController (true, null);
		}

		public void DismissMeasurementsViewController ()
		{
			var test = MarkerUnit.dt;


			if (MeasurementsViewController.isMarkerMeasurement) {
				if (MeasurementsViewController.SelectedMeasurement != "Enable/Disable Markers")
					test = (MarkerUnit)Enum.Parse (typeof(MarkerUnit), MeasurementsViewController.SelectedMeasurement, true);
				else
					test = MarkerUnit.EnableDisableMarkers; 

				markerMeasurements [MeasurementsViewController.SelectedChannel].SelectedUnit = test;
			}
			else
				signalMeasurements [MeasurementsViewController.SelectedChannel].SelectedUnit = (SignalUnit)Enum.Parse (typeof(SignalUnit), MeasurementsViewController.SelectedMeasurement, true);

			measurementsViewController.DismissViewController (true, null);
			UpdateScopeControls ();
		}

		#endregion


		private void UpdateScopeControls ()
		{
			UpdateSelectedChannel ();
			UpdateChannel1UI ();
			UpdateChannel2UI ();
			UpdateTriggerUI ();
			UpdateMeasurement1UI ();
			UpdateMeasurement2UI ();
		}


		void UpdateChannel1UI ()
		{
			UpdateInputCoupling1 ();
			UpdateAttenuationFactor1 ();
			UpdateVoltText1 ();
		}

		void UpdateChannel2UI ()
		{
			UpdateInputCoupling2 ();
			UpdateAttenuationFactor2 ();
			UpdateVoltText2 ();
		}

		void UpdateTriggerUI ()
		{
			UpdateTriggerChannelUI ();
			UpdateTriggerSlopeUI ();
			UpdateTriggerModeUI ();
			UpdateHoldUI ();
			UpdateAutorangeUI ();
			UpdateTimeBaseText ();
		}

		void UpdateMeasurement1UI ()
		{
			UpdateMarkerMeasurement1 ();
			UpdateSignalMeasurement1 ();
		}

		void UpdateMeasurement2UI ()
		{
			UpdateMarkerMeasurement2 ();
			UpdateSignalMeasurement2 ();
		}

		void UpdateSelectedChannel ()
		{
			if (ScopeView.SelectedChannel == Oscilloscope.Channels [0]) {
				btnSelectChannel1.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/CHANNEL 1/CHAN1-ON-6x6.png"), UIControlState.Normal);
				btnSelectChannel2.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/CHANNEL 2/CHAN2-OFF-6x710.png"), UIControlState.Normal);
			} else {
				btnSelectChannel1.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/CHANNEL 1/CHAN1-OFF-6x6.png"), UIControlState.Normal);
				btnSelectChannel2.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/CHANNEL 2/CHAN2-ON-6x710.png"), UIControlState.Normal);
			}
		}

		void UpdateInputCoupling1 ()
		{
			switch (Oscilloscope.Channels [0].InputCoupling) {
			case InputCoupling.AC:
				btnAC1.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/CHANNEL 1/CHAN1-AC-ON-129x6.png"), UIControlState.Normal);
				btnDC1.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/CHANNEL 1/CHAN1-DC-OFF-196x6.png"), UIControlState.Normal);
				btnGND1.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/CHANNEL 1/CHAN1-GND-OFF-263x6.png"), UIControlState.Normal);
				break;
			case InputCoupling.DC:
				btnAC1.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/CHANNEL 1/CHAN1-AC-OFF-129x6.png"), UIControlState.Normal);
				btnDC1.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/CHANNEL 1/CHAN1-DC-ON-196x6.png"), UIControlState.Normal);
				btnGND1.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/CHANNEL 1/CHAN1-GND-OFF-263x6.png"), UIControlState.Normal);
				break;
			case InputCoupling.GND:
				btnAC1.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/CHANNEL 1/CHAN1-AC-OFF-129x6.png"), UIControlState.Normal);
				btnDC1.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/CHANNEL 1/CHAN1-DC-OFF-196x6.png"), UIControlState.Normal);
				btnGND1.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/CHANNEL 1/CHAN1-GND-ON-263x6.png"), UIControlState.Normal);
				break;
			default:
				break;
			}
		}

		void UpdateAttenuationFactor1 ()
		{
			switch (Oscilloscope.Channels [0].AttenuationFactor) {
			case AttenuationFactor.X1:
				btnProbe1.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/CHANNEL 1/CHAN1-1X-OFF-344x6.png"), UIControlState.Normal);
				break;
			case AttenuationFactor.X10:
				btnProbe1.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/CHANNEL 1/CHAN1-10X-ON-344x6.png"), UIControlState.Normal);
				break;
			default:
				break;
			}
		}

		void UpdateVoltText1 ()
		{
			lblVolt1.Text = VoltsPerDivisionConverter.ToString (Oscilloscope.Channels [0].VoltsPerDivision);
		}

		void UpdateInputCoupling2 ()
		{
			switch (Oscilloscope.Channels [1].InputCoupling) {
			case InputCoupling.AC:
				btnAC2.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/CHANNEL 2/CHAN2-AC-ON-129x710.png"), UIControlState.Normal);
				btnDC2.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/CHANNEL 2/CHAN2-DC-OFF-196x710.png"), UIControlState.Normal);
				btnGND2.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/CHANNEL 2/CHAN2-GND-OFF-263x710.png"), UIControlState.Normal);
				break;
			case InputCoupling.DC:
				btnAC2.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/CHANNEL 2/CHAN2-AC-OFF-129x710.png"), UIControlState.Normal);
				btnDC2.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/CHANNEL 2/CHAN2-DC-ON-196x710.png"), UIControlState.Normal);
				btnGND2.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/CHANNEL 2/CHAN2-GND-OFF-263x710.png"), UIControlState.Normal);
				break;
			case InputCoupling.GND:
				btnAC2.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/CHANNEL 2/CHAN2-AC-OFF-129x710.png"), UIControlState.Normal);
				btnDC2.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/CHANNEL 2/CHAN2-DC-OFF-196x710.png"), UIControlState.Normal);
				btnGND2.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/CHANNEL 2/CHAN2-GND-ON-263x710.png"), UIControlState.Normal);
				break;
			default:
				break;
			}
		}

		void UpdateAttenuationFactor2 ()
		{
			switch (Oscilloscope.Channels [1].AttenuationFactor) {
			case AttenuationFactor.X1:
				btnProbe2.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/CHANNEL 2/CHAN2-1X-OFF-344x710.png"), UIControlState.Normal);
				break;
			case AttenuationFactor.X10:
				btnProbe2.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/CHANNEL 2/CHAN2-10X-ON-344x710.png"), UIControlState.Normal);
				break;
			default:
				break;
			}
		}

		void UpdateVoltText2 ()
		{
			lblVolt2.Text = VoltsPerDivisionConverter.ToString (Oscilloscope.Channels [1].VoltsPerDivision);
		}

		void UpdateTriggerChannelUI ()
		{
			if (Oscilloscope.Trigger.Channel == 0) {
				btnTriggerCH1.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/TRIGGER/TRIG-CHAN1-ON-6x96.png"), UIControlState.Normal);
				btnTriggerCH2.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/TRIGGER/TRIG-CHAN2-OFF-60x96.png"), UIControlState.Normal);
			} else {
				btnTriggerCH1.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/TRIGGER/TRIG-CHAN1-OFF-6x96.png"), UIControlState.Normal);
				btnTriggerCH2.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/TRIGGER/TRIG-CHAN2-ON-60x96.png"), UIControlState.Normal);
			}
		}

		void UpdateTriggerSlopeUI ()
		{
			if (Oscilloscope.Trigger.Slope == TriggerSlope.Rising) {
				btnTriggerSlopeUp.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/TRIGGER/TRIG-UP-ON-6x156.png"), UIControlState.Normal);
				btnTriggerSlopeDown.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/TRIGGER/TRIG-DOWN-OFF-60x156.png"), UIControlState.Normal);
			} else {
				btnTriggerSlopeUp.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/TRIGGER/TRIG-UP-OFF-6x156.png"), UIControlState.Normal);
				btnTriggerSlopeDown.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/TRIGGER/TRIG-DOWN-ON-60x156.png"), UIControlState.Normal);
			}
		}

		void UpdateTriggerModeUI ()
		{
			switch (Oscilloscope.Trigger.Mode) {
			case TriggerMode.Normal:
				btnTriggerNrml.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/TRIGGER/TRIG-NRML-ON-6x276.png"), UIControlState.Normal);
				btnTriggerOnce.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/TRIGGER/TRIG-ONCE-OFF-60x276.png"), UIControlState.Normal);
				btnTriggerRun.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/TRIGGER/TRIG-RUN-OFF-6x216.png"), UIControlState.Normal);
				break;
			case TriggerMode.Once:
				btnTriggerNrml.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/TRIGGER/TRIG-NRML-OFF-6x276.png"), UIControlState.Normal);
				btnTriggerOnce.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/TRIGGER/TRIG-ONCE-ON-60x276.png"), UIControlState.Normal);
				btnTriggerRun.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/TRIGGER/TRIG-RUN-OFF-6x216.png"), UIControlState.Normal);
				break;
			case TriggerMode.Run:
				btnTriggerNrml.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/TRIGGER/TRIG-NRML-OFF-6x276.png"), UIControlState.Normal);
				btnTriggerOnce.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/TRIGGER/TRIG-ONCE-OFF-60x276.png"), UIControlState.Normal);
				btnTriggerRun.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/TRIGGER/TRIG-RUN-ON-6x216.png"), UIControlState.Normal);
				break;
			default:
				break;
			}
		}

		void UpdateHoldUI ()
		{
			if (Oscilloscope.Hold)
				btnTriggerHold.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/TRIGGER/TRIG-HOLD-ON-6x336.png"), UIControlState.Normal);
			else
				btnTriggerHold.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/TRIGGER/TRIG-HOLD-OFF-6x336.png"), UIControlState.Normal);
		}

		void UpdateAutorangeUI ()
		{
			if (Oscilloscope.AutoRange)
				btnAutorange.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/AUTO RANGE/AUTORANGE-ON-6x541.png"), UIControlState.Normal);
			else
				btnAutorange.SetBackgroundImage (UIImage.FromBundle ("BUTTONS/AUTO RANGE/AUTORANGE-OFF-6x541.png"), UIControlState.Normal);
		}

		void UpdateTimeBaseText ()
		{
			lblTime.Text = TimeBaseConverter.ToString (Oscilloscope.TimeBase);
		}

		private void SetSignalWithDtValue (int channel)
		{
			var value = MarkerDataCalculator.CalculateTime (Oscilloscope.TimeBase, ScopeView.xMarkers [0].Value, ScopeView.xMarkers [1].Value, Oscilloscope.DeviceContext, ScopeView.Frame);
			value = Math.Round (value, 6);
			var title = ToEngineeringNotation (value);
			title += "s";
			if (channel == 0)
				btnMarkerMeasurements.SetTitle (title, UIControlState.Normal);
			else
				btnMarkerMeasurements2.SetTitle (title, UIControlState.Normal);
		}

		void SetSignalWithFrequency (int channel)
		{
			var value = MarkerDataCalculator.CalculateFrequency (Oscilloscope.TimeBase, ScopeView.xMarkers [0].Value, ScopeView.xMarkers [1].Value, Oscilloscope.DeviceContext, ScopeView.Frame);
			value = Math.Round (value, 2);
			var title = ToEngineeringNotation (value);
			title += "Hz";
			if (channel == 0)
				btnMarkerMeasurements.SetTitle (title, UIControlState.Normal);
			else
				btnMarkerMeasurements2.SetTitle (title, UIControlState.Normal);
		}



		void SetSignalWithDV1 (int channel)
		{
			var value = MarkerDataCalculator.CalculateDV (Oscilloscope.Channels [0].VoltsPerDivision, ScopeView.yMarkers [0].Value, ScopeView.yMarkers [1].Value, Oscilloscope.DeviceContext, ScopeView.Frame);
			value = Math.Round (value, 2);
			var title = ToEngineeringNotation (value);
			title += "V";
			if (channel == 0)
				btnMarkerMeasurements.SetTitle (title, UIControlState.Normal);
			else
				btnMarkerMeasurements2.SetTitle (title, UIControlState.Normal);
		}

		void SetSignalWithDV2 (int channel)
		{
			var value = MarkerDataCalculator.CalculateDV (Oscilloscope.Channels [1].VoltsPerDivision, ScopeView.yMarkers [0].Value, ScopeView.yMarkers [1].Value, Oscilloscope.DeviceContext, ScopeView.Frame);
			value = Math.Round (value, 2);
			var title = ToEngineeringNotation (value);
			title += "V";
			if (channel == 0)
				btnMarkerMeasurements.SetTitle (title, UIControlState.Normal);
			else
				btnMarkerMeasurements2.SetTitle (title, UIControlState.Normal);

		}

		void EnableDisableMarkers ()
		{
			ScopeView.ToggleMarkers ();
		}

		void UpdateMarkerMeasurement1 ()
		{
			switch (markerMeasurements [0].SelectedUnit) {
			case MarkerUnit.dt:
				SetSignalWithDtValue (0);
				break;
			case MarkerUnit.Frequency:
				SetSignalWithFrequency (0);
				break;
			case MarkerUnit.dV1:
				SetSignalWithDV1 (0);
				break;
			case MarkerUnit.dV2:
				SetSignalWithDV2 (0);
				break;
			case MarkerUnit.EnableDisableMarkers:
				EnableDisableMarkers ();
				markerMeasurements [0].SelectedUnit = MarkerUnit.dt;
				break;
			default:
				btnMarkerMeasurements.SetTitle ("Unsupported Measurement", UIControlState.Normal);
				break;
			}
		}

		void UpdateMarkerMeasurement2 ()
		{
			switch (markerMeasurements [1].SelectedUnit) {
			case MarkerUnit.dt:
				SetSignalWithDtValue (1);
				break;
			case MarkerUnit.Frequency:
				SetSignalWithFrequency (1);
				break;
			case MarkerUnit.dV1:
				SetSignalWithDV1 (1);
				break;
			case MarkerUnit.dV2:
				SetSignalWithDV2 (1);
				break;
			case MarkerUnit.EnableDisableMarkers:
				EnableDisableMarkers ();
				markerMeasurements [1].SelectedUnit = MarkerUnit.dt;
				break;
			default:
				btnMarkerMeasurements.SetTitle ("Unsupported Measurement", UIControlState.Normal);
				break;
			}
		}

		public string GetMeasurementString (SignalUnit unit, int channel)
		{
			switch (unit) {
			case SignalUnit.DbGain:
				return Oscilloscope.DBGain ().ToString ();
			case SignalUnit.Dbm1:
				return Oscilloscope.Channels [channel].DBm ().ToString ();
			case SignalUnit.Dbm2:
				return Oscilloscope.Channels [channel].DBm ().ToString();
			case SignalUnit.RMS:
				return Oscilloscope.Channels [channel].Vrms().ToString();
			case SignalUnit.TRMS:
				return Oscilloscope.Channels [channel].VTrms().ToString();
			case SignalUnit.Vdc:
				return Oscilloscope.Channels [channel].Vdc().ToString();
			case SignalUnit.VMax:
				return Oscilloscope.Channels [channel].Vmax().ToString();
			case SignalUnit.VMin:
				return Oscilloscope.Channels [channel].Vmin().ToString();
			case SignalUnit.Vptp:
				return Oscilloscope.Channels [channel].Vptp().ToString();
			case SignalUnit.WRMS16:
				return Oscilloscope.Channels [channel].Wrms16().ToString();
			case SignalUnit.WRMS2:
				return Oscilloscope.Channels [channel].Wrms2().ToString();
			case SignalUnit.WRMS32:
				return Oscilloscope.Channels [channel].Wrms32().ToString();
			case SignalUnit.WRMS4:
				return Oscilloscope.Channels [channel].Wrms4().ToString();
			case SignalUnit.WRMS8:
				return Oscilloscope.Channels [channel].Wrms8().ToString();
			default:
				return "?";
			}
		}

		void UpdateSignalMeasurement1 ()
		{
			btnSignalMeasurements.SetTitle (GetMeasurementString (signalMeasurements [0].SelectedUnit, 0), UIControlState.Normal);
		}

		void UpdateSignalMeasurement2 ()
		{
			btnSignalMeasurements2.SetTitle (GetMeasurementString (signalMeasurements [0].SelectedUnit, 1), UIControlState.Normal);
		}

		private static string ToEngineeringNotation (double d)
		{
			double exponent = Math.Log10 (Math.Abs (d));
			if (Math.Abs (d) >= 1) {
				switch ((int)Math.Floor (exponent)) {
				case 0:
				case 1:
				case 2:
					return d.ToString ();
				case 3:
				case 4:
				case 5:
					return (d / 1e3).ToString () + "k";
				case 6:
				case 7:
				case 8:
					return (d / 1e6).ToString () + "M";
				case 9:
				case 10:
				case 11:
				default:
					return (d / 1e24).ToString () + "Y";
				}
			} else if (Math.Abs (d) > 0) {
				switch ((int)Math.Floor (exponent)) {
				case -1:
				case -2:
				case -3:
					return (d * 1e3).ToString () + "m";
				case -4:
				case -5:
				case -6:
					return (d * 1e6).ToString () + "μ";
				case -7:
				case -8:
				case -9:
					return (d * 1e9).ToString () + "n";
				default:
					return (d * 1e15).ToString () + "y";
				}
			} else {
				return "0";
			}
		}
	}
}
