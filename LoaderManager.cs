using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using VRUI;
using VRUIControls;
using TMPro;
using IllusionPlugin;
using UnityEngine.Events;
using HMUI;
using System.Text;
using System.Threading;
using SongLoaderPlugin;
using SongLoaderPlugin.OverrideClasses;

namespace MediocreLoader
{
    public class LoaderManager : MonoBehaviour
    {
        public static LoaderManager Instance = null;
        MainMenuViewController mainMenuViewController = null;
        MainFlowCoordinator flowCoordinator = null;
        LevelListViewController listViewController = null;
        StandardLevelDetailViewController detailViewController = null;
        SoloModeSelectionViewController soloModeSelectionViewController;
        SoloFreePlayFlowCoordinator soloFreePlayFlowCoordinator = null;

        StandardLevelReturnToMenuController standardLevelReturnToMenu = null;

        VRUINavigationController vruicontroller = null;
        GameScenesManager gameScenesManager = null;
        static PauseMenuManager pauseMenuManager;
        BeatmapDifficultyViewController beatmapDifficultyViewController = null;
        LevelListTableView levelListView = null;
        GameplaySetupViewController gameplaySetupViewController = null;
        static StandardLevelGameplayManager gameplayManager;
        PracticeViewController practiceController = null;
        TableView tableView = null;
        HealthWarningMenuController fuckthis;

        SongPreviewPlayer player = null;

        public Action songEnd;



        private Dictionary<string, int> _weights = new Dictionary<string, int>
        {
            ["Level4"] = 11,
            ["Level2"] = 10,
            ["Level9"] = 9,
            ["Level5"] = 8,
            ["Level10"] = 7,
            ["Level6"] = 6,
            ["Level7"] = 5,
            ["Level1"] = 4,
            ["Level3"] = 3,
            ["Level8"] = 2,
            ["Level11"] = 1,

            ["Level4OneSaber"] = 12,
            ["Level1OneSaber"] = 11,
            ["Level2OneSaber"] = 10,
            ["Level9OneSaber"] = 9,
            ["Level7OneSaber"] = 8,
        };
        

        private Dictionary<string, BeatmapDifficulty> _difficulties = new Dictionary<string, BeatmapDifficulty>
        {
            ["Easy"] = BeatmapDifficulty.Easy,
            ["Normal"] = BeatmapDifficulty.Normal,
            ["Hard"] = BeatmapDifficulty.Hard,
            ["Expert"] = BeatmapDifficulty.Expert,
            ["ExpertPlus"] = BeatmapDifficulty.ExpertPlus
        };

        static bool autoPlay = true;
        static string songName = "";
        static string author = "";
        static string subName = "";
        static string difficulty = "";
        static Vector2 pos = new Vector2(65.0f, 74.0f);
        static float time = 0f;
        static bool autoPlayBuffer = true;
        static bool playSong = false;
        static float playbackSpeed = 1f;

        const string excludeStandardSetting = "excludeStandard";
        const string autoPlaySetting = "autoPlay";

        IBeatmapLevel level = null;
        IDifficultyBeatmap difficultyLevel = null;
        private static bool loadSong = false;
        public static void OnLoad()
        {
            if (Instance != null) return;
            new GameObject("Random Song Manager").AddComponent<LoaderManager>();

        }

        public static bool isMenuScene(Scene scene)
        {
            return (scene.name == "Menu");
        }

        public static bool isGameScene(Scene scene)
        {

            return (scene.name == "GameCore");
        }
       
        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;
                DontDestroyOnLoad(gameObject);
                autoPlay = true;
                WatsonTcpServer server = new WatsonTcpServer("127.0.0.1", 14779, ClientConnected, ClientDisconnected, MessageReceived, true);
            }
            else
            {
                Destroy(this);
            }
        }

        public void SceneManagerOnActiveSceneChanged(Scene arg0, Scene scene)
        {
            if (isMenuScene(scene))
            {
                try
                {
                    flowCoordinator = Resources.FindObjectsOfTypeAll<MainFlowCoordinator>().FirstOrDefault();

                    mainMenuViewController = flowCoordinator.GetPrivateField<MainMenuViewController>("_mainMenuViewController");
                    //soloModeSelectionViewController =   flowCoordinator.GetPrivateField<SoloModeSelectionViewController>("_soloFreePlayFlowCoordinator");


                    soloFreePlayFlowCoordinator = flowCoordinator.GetPrivateField<SoloFreePlayFlowCoordinator>("_soloFreePlayFlowCoordinator");
                    detailViewController = soloFreePlayFlowCoordinator.GetPrivateField<StandardLevelDetailViewController>("_levelDetailViewController");
                    gameplaySetupViewController = soloFreePlayFlowCoordinator.GetPrivateField<GameplaySetupViewController>("_gameplaySetupViewController");
                    practiceController = soloFreePlayFlowCoordinator.GetPrivateField<PracticeViewController>("_practiceViewController");
                    beatmapDifficultyViewController = soloFreePlayFlowCoordinator.GetPrivateField<BeatmapDifficultyViewController>("_beatmapDifficultyViewControllerViewController");
                    listViewController = soloFreePlayFlowCoordinator.GetPrivateField<LevelListViewController>("_levelListViewController");

                    levelListView = listViewController.GetPrivateField<LevelListTableView>("_levelListTableView");
                    tableView = levelListView.GetPrivateField<TableView>("_tableView");

                    gameScenesManager = Resources.FindObjectsOfTypeAll<GameScenesManager>().FirstOrDefault();

                    standardLevelReturnToMenu = Resources.FindObjectsOfTypeAll<StandardLevelReturnToMenuController>().FirstOrDefault();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
                
            }
            if (isGameScene(scene))
            {
                
                
                if (autoPlay)
                {
                    
                }
            }
            
            Console.WriteLine(scene.name);
        }
        private void getSong(string name, string author, string subname, string difficulty)
        {
            Console.WriteLine("Attempting to find song: " + name);
            

            SelectAndLoadSong(name, difficulty);
        }
        
        private float[] Difficulties()
        {
            return new float[] {
                (float)BeatmapDifficulty.Easy,
                (float)BeatmapDifficulty.Normal,
                (float)BeatmapDifficulty.Hard,
                (float)BeatmapDifficulty.Expert,
                (float)BeatmapDifficulty.ExpertPlus
            };
        }

        

        private void Update()
        {
            if (loadSong && isMenuScene(SceneManager.GetActiveScene()))
            {
                try
                {
                    SongLoader.Instance.RefreshSongs(true);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Can't refresh songs! EXCEPTION: " + e);
                }
                playSong = true;
                loadSong = false;
            }
            if (playSong && SongLoader.AreSongsLoaded && isMenuScene(SceneManager.GetActiveScene()))
            { 
                Console.WriteLine("attempting song load");
                playSong = false;
                getSong(songName, author, subName, difficulty);
                autoPlay = autoPlayBuffer;
            }
            if (!autoPlay && isGameScene(SceneManager.GetActiveScene()))
            {
                pauseMenuManager = Resources.FindObjectsOfTypeAll<PauseMenuManager>().First();
                gameplayManager = Resources.FindObjectsOfTypeAll<StandardLevelGameplayManager>().First();

                gameplayManager.HandlePauseTriggered();
                if (pauseMenuManager.isActiveAndEnabled)
                {
                    autoPlay = true;
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape) && isGameScene(SceneManager.GetActiveScene()))
            {
                pauseMenuManager = Resources.FindObjectsOfTypeAll<PauseMenuManager>().First();
                gameplayManager = Resources.FindObjectsOfTypeAll<StandardLevelGameplayManager>().First();
                gameplayManager.HandlePauseTriggered();
                pauseMenuManager.MenuButtonPressed();

            }
            if (SceneManager.GetActiveScene().name == "HealthWarning")
            {
                fuckthis = Resources.FindObjectsOfTypeAll<HealthWarningMenuController>().FirstOrDefault();
                fuckthis.ContinueButtonPressed();
            }
        }




 

        void SelectAndLoadSong(string name,string difficulty)
        {




            //try
            //{
            //    mainMenuViewController.HandleMenuButton(MainMenuViewController.MenuButton.SoloFreePlay);
            //}
            //catch (Exception e)
            //{

            //}
            //try
            //{
            //    soloModeSelectionViewController.HandleMenuButton(SoloModeSelectionViewController.MenuType.FreePlayMode);
            //}
            //catch (Exception e)
            //{

            //}
            LevelCollectionSO _levelCollection = SongLoader.CustomLevelCollectionSO;
            LevelSO level = _levelCollection.GetLevelsWithBeatmapCharacteristic(Resources.FindObjectsOfTypeAll<BeatmapCharacteristicSO>().First(x => x.characteristicName == "Standard")).First(x => x.songName == name);

            //level = listViewController.GetPrivateField<IBeatmapLevel[]>("_levels").Where(x => x.songName == name)// && x.songAuthorName == author && x.songSubName == subname)
            //    .ToList().ElementAt(0);

            

            Console.WriteLine("Song found:" + level.songName);

            difficultyLevel = level.GetDifficultyBeatmap(_difficulties[difficulty]);
              //////////////////////////////////////////////////////////////////
             //            THING TO GET SONG BY JUST STARTING IT             //
            //////////////////////////////////////////////////////////////////
            GameplayModifiers gameplayModifiers = new GameplayModifiers();
            gameplayModifiers.ResetToDefault();
            gameplayModifiers.noFail = true;
            PlayerSpecificSettings playerSettings = Resources.FindObjectsOfTypeAll<PlayerDataModelSO>().FirstOrDefault().currentLocalPlayer.playerSpecificSettings;

            var practiceSettings = new PracticeSettings(PracticeSettings.defaultPracticeSettings);
            practiceSettings.startSongTime = time;
            practiceSettings.songSpeedMul = playbackSpeed;

            MenuSceneSetupDataSO menu = Resources.FindObjectsOfTypeAll<MenuSceneSetupDataSO>().First();
            SongLoader loader = Resources.FindObjectsOfTypeAll<SongLoader>().First();
            loader.LoadAudioClipForLevel((CustomLevel)level, delegate (CustomLevel customLevel)
            {
                menu.StartStandardLevel(difficultyLevel, gameplayModifiers, playerSettings, practiceSettings, null, null);
            });
            
              ///////////////////////////////////////////////////////////////////
             //          THING TO GET SONG BY NAVIGATING THROUGH MENUS        //
            ///////////////////////////////////////////////////////////////////
            //MenuSceneSetupDataSO menu = Resources.FindObjectsOfTypeAll<MenuSceneSetupDataSO>().First();

            //soloFreePlayFlowCoordinator.HandleLevelListViewControllerDidSelectLevel(listViewController, level);
            //levelListView.SelectAndScrollToLevel(level.levelID);
            //int row = levelListView.RowNumberForLevelID(level.levelID);
            //levelListView.HandleDidSelectRowEvent(levelListView.GetPrivateField<TableView>("_tableView"), row);
            //try
            //{

            //    DifficultyTableView difficultyTableView = beatmapDifficultyViewController.GetPrivateField<DifficultyTableView>("_difficultyTableView");
            //    TableView tableView = difficultyTableView.GetPrivateField<TableView>("_tableView");
            //    difficultyTableView.HandleDidSelectRowEvent(tableView, 0);
            //    tableView.SelectRow(0);
            //    difficultyTableView.SelectRow(difficultyLevel, false);
            //    soloFreePlayFlowCoordinator.HandleDifficultyViewControllerDidSelectDifficulty(beatmapDifficultyViewController, difficultyLevel);

            //    practiceController.Init(level, new PracticeSettings());

            //    GameplayModifiers gameplayModifiers = new GameplayModifiers();
            //    gameplayModifiers.ResetToDefault();
            //    gameplayModifiers.noFail = true;

            //    gameplaySetupViewController.SetData(gameplaySetupViewController.playerSettings, gameplayModifiers);
            //    soloFreePlayFlowCoordinator.HandleLevelDetailViewControllerDidPressPracticeButton(detailViewController);
            //    Console.WriteLine("loading " + difficultyLevel.level.songName);
            //    detailViewController.PracticeButtonPressed();

            //    if (!autoPlayBuffer && time > 2)
            //    {   // pause on start
            //        practiceController.HandleSongStartScrollbarOnValueChanged(time - 2);
            //    }
            //    else
            //    {   // autoplay or negative time will fuck it up
            //        practiceController.HandleSongStartScrollbarOnValueChanged(time);
            //    }
            //    Console.WriteLine("Starting song at: " + practiceController.GetPrivateField<PracticeSettings>("_practiceSettings").startSongTime);
            //    practiceController.PlayButtonPressed();
            //    soloFreePlayFlowCoordinator.HandlePracticeViewControllerDidPressPlayButton();
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //    //Console.WriteLine(e.StackTrace);
            //}
            //finally
            //{
            //    Console.WriteLine("[Mediocre Loader] Finally");
            //}

            //mainGameSceneSetupData.TransitionToScene(0.7f);
        }


        static bool ClientConnected(string ipPort)
        {
            Console.WriteLine("Client connected: " + ipPort);
            return true;
        }
        static bool ClientDisconnected(string ipPort)
        {
            Console.WriteLine("Client disconnected: " + ipPort);
            return true;
        }
        static bool MessageReceived(string ipPort, byte[] data)
        {
            string msg = "";
            if (data != null && data.Length > 0) msg = Encoding.UTF8.GetString(data);
            var info = msg.Split(new string[] { ":::" }, StringSplitOptions.None);
            songName = info[0];
            author = info[1];
            subName = info[2];
            difficulty = info[3];
            time = float.Parse(info[4]);
            autoPlay = bool.Parse(info[5]);
            playbackSpeed = float.Parse(info[6]);

            if (isGameScene(SceneManager.GetActiveScene()))
            {
                pauseMenuManager = Resources.FindObjectsOfTypeAll<PauseMenuManager>().First();
                gameplayManager = Resources.FindObjectsOfTypeAll<StandardLevelGameplayManager>().First();
                gameplayManager.Pause();
                pauseMenuManager.MenuButtonPressed();
            }
            loadSong = true;
            
            autoPlayBuffer = autoPlay;
            //try
            //{
            //    SongLoader.Instance.RefreshSongs(true);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("Can't refresh songs! EXCEPTION: " + e);
            //}
            return true;
        }
        private void HandleMainGameSceneDidFinish()
        {
            if (!isGameScene( SceneManager.GetActiveScene()))
            {
                gameScenesManager.transitionDidFinishEvent -= HandleMainGameSceneDidFinish;
                
            }
            
            //standardLevelReturnToMenu.ReturnToMenu();
            //coordinator.HandleStandardLevelDidFinish(StandardLevelSceneSetupDataSO standardLevelSceneSetupData, LevelCompletionResults levelCompletionResults);
        }
        //private IEnumerator pauseGame(StandardLevelGameplayManager manager)
        //{
        //    yield return new WaitForSeconds(2);

        //}
        //public static void LogComponents(Transform t, string prefix = "=", bool includeScipts = false)
        //{
        //    Console.WriteLine(prefix + ">" + t.name);

        //    if (includeScipts)
        //    {
        //        foreach (var comp in t.GetComponents<MonoBehaviour>())
        //        {
        //            Console.WriteLine(prefix + "-->" + comp.GetType());
        //        }
        //    }

        //    foreach (Transform child in t)
        //    {
        //        LogComponents(child, prefix + "=", includeScipts);
        //    }
        //}
    }

    //public static class LevelExtenstion
    //{
    //    public static bool HasDifficultyInRange(this IStandardLevel level, LevelDifficulty min, LevelDifficulty max)
    //    {
    //        bool hasDiff = false;
    //        for (int i = (int)min; i <= (int)max; i++)
    //        {
    //            if (level.GetDifficultyLevel((LevelDifficulty)i) != null)
    //            {
    //                hasDiff = true;
    //            }
    //        }
    //        return hasDiff;
    //    }
    //}
}

