using System.Collections.Generic;
using System.Linq;
using Managers;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour, IDisabledUI
{
	public GameObject textPrefab;
	public Transform parent;
	public Button readyUp;
	public Button customizeButton;
	public Text trackNameText;
	public Text modeNameText;
	public Text lobbyNameText;
	public Dropdown trackNameDropdown;
	public Dropdown gameTypeDropdown;
	public Image trackIconImage;

	private static readonly Dictionary<RoomPlayer, LobbyItemUI> ListItems = new Dictionary<RoomPlayer, LobbyItemUI>();
	private static bool IsSubscribed;

	private void Awake()
	{
		trackNameDropdown.onValueChanged.AddListener(x =>
		{
			var gm = GameManager.Instance;
			if (gm != null) gm.TrackId = x;
		});
		gameTypeDropdown.onValueChanged.AddListener(x =>
		{
			var gm = GameManager.Instance;
			if (gm != null) gm.GameTypeId = x;
		});

		GameManager.OnLobbyDetailsUpdated += UpdateDetails;

		RoomPlayer.PlayerChanged += (player) =>
		{
			var isLeader = RoomPlayer.Local.IsLeader;
			trackNameDropdown.interactable = isLeader;
			gameTypeDropdown.interactable = isLeader;
			customizeButton.interactable = !RoomPlayer.Local.IsReady;
		};
	}

	void UpdateDetails(GameManager manager)
	{
		Debug.Log("LobbyUI UpdateDetails>>");
		lobbyNameText.text = "Room Code: " + manager.LobbyName;
		trackNameText.text = manager.TrackName;
		modeNameText.text = manager.ModeName;

		var tracks = ResourceManager.Instance.tracks;
		var trackOptions = tracks.Select(x => x.trackName).ToList();

		var gameTypes = ResourceManager.Instance.gameTypes;
		var gameTypeOptions = gameTypes.Select(x => x.modeName).ToList();

		trackNameDropdown.ClearOptions();
		trackNameDropdown.AddOptions(trackOptions);
		trackNameDropdown.value = GameManager.Instance.TrackId;

		trackIconImage.sprite = ResourceManager.Instance.tracks[GameManager.Instance.TrackId].trackIcon;

		gameTypeDropdown.ClearOptions();
		gameTypeDropdown.AddOptions(gameTypeOptions);
		gameTypeDropdown.value = GameManager.Instance.GameTypeId;
	}

	public void Setup()
	{
		if (IsSubscribed) return;

		RoomPlayer.PlayerJoined += AddPlayer;
		RoomPlayer.PlayerLeft += RemovePlayer;

		RoomPlayer.PlayerChanged += EnsureAllPlayersReady;

		readyUp.onClick.AddListener(ReadyUpListener);

		IsSubscribed = true;

		Debug.Log("LobbyUI Setup>>");
	}

	private void OnDestroy()
	{
		if (!IsSubscribed) return;

		RoomPlayer.PlayerJoined -= AddPlayer;
		RoomPlayer.PlayerLeft -= RemovePlayer;

		readyUp.onClick.RemoveListener(ReadyUpListener);

		IsSubscribed = false;
	}

	private void AddPlayer(RoomPlayer player)
	{
		if (ListItems.ContainsKey(player))
		{
			var toRemove = ListItems[player];
			Destroy(toRemove.gameObject);

			ListItems.Remove(player);
		}

		var obj = Instantiate(textPrefab, parent).GetComponent<LobbyItemUI>();
		obj.SetPlayer(player);

		ListItems.Add(player, obj);
		Debug.Log("LobbyUI PlayerAdded AddPlayer>>" + player.KartId);
		
		UpdateDetails(GameManager.Instance);
	}

	private void RemovePlayer(RoomPlayer player)
	{
		if (!ListItems.ContainsKey(player))
			return;

		var obj = ListItems[player];
		if (obj != null)
		{
			Destroy(obj.gameObject);
            Debug.Log("LobbyUI RemovePlayer>>");
            ListItems.Remove(player);
		}
	}

	public void OnDestruction()
	{
	}

	private void ReadyUpListener()
	{
		var local = RoomPlayer.Local;
        if (local && local.Object && local.Object.IsValid)
        {
			Debug.Log("LobbyUI ReadyUpListener LocalPlayer ReadyUp>>" + local.transform.name);
            local.RPC_ChangeReadyState(!local.IsReady);
        }
	}

	private void EnsureAllPlayersReady(RoomPlayer lobbyPlayer)
	{
		if (!RoomPlayer.Local.IsLeader) 
			return;

		if (IsAllReady())
		{
			int scene = ResourceManager.Instance.tracks[GameManager.Instance.TrackId].buildIndex;

			Debug.Log("LobbyUI IsAllReady()>> LevelManager.LoadTrack:" + scene);
			LevelManager.LoadTrack(scene);
		}
	}

	private static bool IsAllReady() => RoomPlayer.Players.Count>0 && RoomPlayer.Players.All(player => player.IsReady);
}