using GameEnumerations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game2
{
    public class GameResources_2 : GeneralController
    {      
        public static GameResources_2 GameResources;
        [HideInInspector] public List<Vector2> RemoteJoystick = new List<Vector2>();
        [SerializeField] private Player _blue, _red;
        private bool _checkingResults = false;

        private readonly int WinScore = 5;

        protected override void Awake()
        {
            base.Awake();
            GameResources = this;
        }

        public void RoundResults()
        {
            if (_checkingResults == false)
            {
                _checkingResults = true;
                PauseGame(PauseTypes.EndRound);
                StartCoroutine(FinishRound());
            }
        }

        private IEnumerator FinishRound()
        {
            yield return null;

            if (_blue.GetPoint && _red.GetPoint)
                UpdateAndTrySendScore(PlayerTypes.Null, GameResults.Draw);
            else if (_blue.GetPoint)
                UpdateAndTrySendScore(_blue.PlayerType, GameResults.Lose);
            else if (_red.GetPoint)
                UpdateAndTrySendScore(_red.PlayerType, GameResults.Lose);

            
            if (_res.GameScore.CheckEndGame(WinScore) == false)
                OpenEndRoundPanel();
        }
        
        protected override void ResetLevel()
        {
            base.ResetLevel();
            _blue.ResetLevel();
            _red.ResetLevel();
            _checkingResults = false;
        }
       
        public void SetControlTypes(ControlTypes blueType, ControlTypes redType)
        {
            _blue.SetControlType(blueType);
            _red.SetControlType(redType);
        }

        public void SendFrame()
        {
            if (Network.ClientUDP != null)
            {
                FrameInfo data = new FrameInfo(_blue, _red);
                Serializer<FrameInfo>.SendMessage(data, ConnectTypes.UDP);
            }              
        }

        public void MoveToPosition(FrameInfo frame)
        {
            Vector3 position1 = new Vector3(frame.X_blue, frame.Y_blue);
            Vector3 position2 = new Vector3(frame.X_red, frame.Y_red);

            _blue._playerMover.SetBroadcastPositions(position1, frame.GetQuaternion(frame.Quaternion_blue));
            _red._playerMover.SetBroadcastPositions(position2, frame.GetQuaternion(frame.Quaternion_red));
        }
    }
}
