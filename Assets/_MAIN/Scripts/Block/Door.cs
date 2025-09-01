using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace NinovaGamesCase.Core
{
    /// <summary>
    /// Represents a door in the game level, which connects multiple nodes
    /// and visually indicates direction and color data.
    /// </summary>
    public class Door : MonoBehaviour
    {
        public LevelDoorData LevelDoorData { get; private set; }
        public Vector3 WorldPosition { get; private set; }

        private Transform _arrow;
        private Transform _mesh;

        public List<Node> CurrentNodes { get; private set; }

        private void Awake()
        {
            _mesh = transform.GetChild(0);
            _arrow = transform.GetChild(1);
        }

        /// <summary>
        /// Initializes the door with data, including direction, length, and visual appearance.
        /// Calculates its position, orientation, affected grid nodes, and scaling.
        /// </summary>
        /// <param name="data">The configuration data for the door.</param>
        public void Initialize(LevelDoorData data)
        {
            LevelDoorData = data;

            var dir = Helpers.Helpers.LevelDoorDirectionToVector(data.Direction);

            CurrentNodes = new List<Node>();
            CurrentNodes.Add(Grid.Instance.GetGrid()[data.GridPosition.x, data.GridPosition.y]);

            var mask = transform.GetChild(2);

            switch (data.Direction)
            {
                case LevelDoorDirection.Up:
                    _arrow.localEulerAngles = new Vector3(90, 270, 0);
                    for (var i = 0; i < data.Length - 1; i++)
                        CurrentNodes.Add(Grid.Instance.GetGrid()[data.GridPosition.x + (i + 1), data.GridPosition.y]);

                    mask.localRotation = Quaternion.Euler(0, 0, 0);
                    break;
                case LevelDoorDirection.Down:
                    _arrow.localEulerAngles = new Vector3(90, 90, 0);
                    for (var i = 0; i < data.Length - 1; i++)
                        CurrentNodes.Add(Grid.Instance.GetGrid()[data.GridPosition.x + (i + 1), data.GridPosition.y]);

                    mask.localRotation = Quaternion.Euler(0, 180, 0);
                    break;
                case LevelDoorDirection.Right:
                    _arrow.localEulerAngles = new Vector3(90, 0, 0);
                    for (var i = 0; i < data.Length - 1; i++)
                        CurrentNodes.Add(Grid.Instance.GetGrid()[data.GridPosition.x, data.GridPosition.y + (i + 1)]);

                    mask.localRotation = Quaternion.Euler(0, 90, 0);
                    break;
                case LevelDoorDirection.Left:
                    _arrow.localEulerAngles = new Vector3(90, 180, 0);
                    for (var i = 0; i < data.Length - 1; i++)
                        CurrentNodes.Add(Grid.Instance.GetGrid()[data.GridPosition.x, data.GridPosition.y + (i + 1)]);

                    mask.localRotation = Quaternion.Euler(0, 270, 0);
                    break;
            }

            var a = (Grid.Instance.GetGrid()[data.GridPosition.x, data.GridPosition.y].WorldPosition + dir * 1f);
            a.y = 0;
            WorldPosition = a;

            var p = WorldPosition;
            var s = Helpers.Helpers.GetSetupPosAndScale(dir, p, _mesh.localScale, data.Length);


            transform.position = s.Item1;
            _mesh.localScale = s.Item2;


            var mm = mask.GetChild(0);
            mm.localScale = new Vector3(data.Length, mm.localScale.y, mm.localScale.z);
            _mesh.GetComponent<MeshRenderer>().material = data.ColorData.ColorMaterial;
        }

        /// <summary>
        /// Plays a sequence of animations when the door is matched,
        /// using DOTween's Sequence for better control.
        /// </summary>
        public void OnMatchedAnim()
        {
            var sequence = DOTween.Sequence();

            sequence.Append(_arrow.DOLocalMoveY(0.1f, 0.1f));
            sequence.Join(_mesh.DOLocalMoveY(-0.5f, 0.1f));
            sequence.Join(_mesh.DOScaleY(0.3f, 0.1f));

            sequence.AppendInterval(1f);

            sequence.Append(_arrow.DOLocalMoveY(0.6f, 0.1f));
            sequence.Join(_mesh.DOLocalMoveY(0f, 0.1f));
            sequence.Join(_mesh.DOScaleY(1f, 0.1f));


            sequence.OnComplete(() => { });
        }
    }
}