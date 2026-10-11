using System;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using Logic;
using Player;
using UnityEngine;
using UI.Settings;

namespace Audio {
	public class AudioManager : MonoBehaviour {
		#region Fields

		public static AudioManager Instance;

		private Bus masterBus, musicBus, ambienceBus, sfxBus, uiBus;

		public enum BusTypes {
			Master,
			Music,
			Ambience,
			SFX,
			UI
		}

		public enum FootstepSurface {
			Grass,
			Dirt
		}

		public enum MusicTrack {
		}

		public enum Parameter {
			FootstepSurface,
			MusicTrack
		}

		private readonly List<EventInstance> eventInstances = new();
		private          EventInstance       playerEventInstance, musicEventInstance;
		private          GameObject          camControllerObject, playerObject;

		[SerializeField]
		private SliderSettings masterBusSetting, musicBusSetting, ambienceBusSetting, sfxBusSetting, uiBusSetting;

		#endregion

		#region Unity Functions

		private void Awake() {
			if (Instance != null && Instance != this) {
				Destroy(gameObject);
				return;
			}

			Instance = this;

			masterBus   = RuntimeManager.GetBus("bus:/");
			musicBus    = RuntimeManager.GetBus("bus:/Music");
			ambienceBus = RuntimeManager.GetBus("bus:/Ambience");
			sfxBus      = RuntimeManager.GetBus("bus:/SFX");
			uiBus       = RuntimeManager.GetBus("bus:/UI");

			camControllerObject = FindAnyObjectByType<CameraManager>().gameObject;
			playerObject        = FindAnyObjectByType<PlayerController>().gameObject;
		}

		#endregion

		private void OnEnable() => SettingService.OnSettingChanged += OnSettingChanged;

		private void OnDestroy() {
			SettingService.OnSettingChanged -= OnSettingChanged;

			foreach (var eventInstance in eventInstances) {
				eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
				eventInstance.release();
			}
		}

		private void OnSettingChanged(string settingId) {
			if (settingId == masterBusSetting.id)
				SetBusVolume(BusTypes.Master, GetSettingValue(masterBusSetting.id));
			else if (settingId == musicBusSetting.id)
				SetBusVolume(BusTypes.Music, GetSettingValue(musicBusSetting.id));
			else if (settingId == ambienceBusSetting.id)
				SetBusVolume(BusTypes.Ambience, GetSettingValue(ambienceBusSetting.id));
			else if (settingId == sfxBusSetting.id)
				SetBusVolume(BusTypes.SFX, GetSettingValue(sfxBusSetting.id));
			else if (settingId == uiBusSetting.id)
				SetBusVolume(BusTypes.UI, GetSettingValue(uiBusSetting.id));
		}

		private float GetSettingValue(string settingId) {
			var value = SettingService.GetInt(settingId, 10);
			return
				value / 10f; //? Return value should be between 0 and 1, so divide by 10 because setting value is 0-10 :)
		}
		
		#region Parameters

		public void SetParameter(Parameter parameter, int value) {
			RuntimeManager.StudioSystem.setParameterByName(parameter.ToString(), value);
		}

		public void SetParameter(Parameter parameter, bool value) => SetParameter(parameter, value ? 1 : 0);

		public void SetFootstepSurface(FootstepSurface surface) =>
			SetParameter(Parameter.FootstepSurface, (int)surface);

		public void SetMusicTrack(MusicTrack track) => SetParameter(Parameter.MusicTrack, (int)track);

		#endregion

		#region Bus Functions

		/// <summary>
		/// Sets the volume of a specified bus.
		/// </summary>
		/// <param name="bus"></param>
		/// <param name="volume">0-1</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public void SetBusVolume(BusTypes bus, float volume) {
			switch (bus) {
				case BusTypes.Master:
					masterBus.setVolume(volume);
					break;
				case BusTypes.Music:
					musicBus.setVolume(volume);
					break;
				case BusTypes.Ambience:
					ambienceBus.setVolume(volume);
					break;
				case BusTypes.SFX:
					sfxBus.setVolume(volume);
					break;
				case BusTypes.UI:
					uiBus.setVolume(volume);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(bus), bus, null);
			}
		}

		#endregion
	}
}