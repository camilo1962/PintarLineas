using System.Collections.Generic;
using UnityEngine;

namespace LinePaint
{
    public class LevelManager_paint : MonoBehaviour
    {
        [SerializeField] private Color[] colors;
        [SerializeField] private Material cubeMat;
        [SerializeField] private CameraZoom gameCamera;
        [SerializeField] private CameraZoom solutionCamera;
        [SerializeField] private float _cellSize;
        [SerializeField] private BrushController brushPefab;
        [SerializeField] private LinePaintScript linePaintPrefab;
        [SerializeField] private Cell cellPrefab;
        [SerializeField] private LevelDataScriptable[] leveldataArray;
        [SerializeField] private UIManager_paint uIManager;

        private List<ConnectedLine> inProgressPattern = new List<ConnectedLine>();
        private List<LinePaintScript> connectedLinePaints = new List<LinePaintScript>();
        private Cell[,] cells;
        private Grid grid;
        private SwipeControl_paint swipeControl;
        private BrushController currentBrush;

        // Start is called before the first frame update
        void Start()
        {
            
            GameManager_paint.currentLevel = PlayerPrefs.GetInt("CurrentLevel", 0);
            GameManager_paint.totalDiamonds = PlayerPrefs.GetInt("TotalDiamonds", 0);
            
            uIManager.TotalDiamonds.text = "" + GameManager_paint.totalDiamonds;
            uIManager.LevelText.text = "Level " + (GameManager_paint.currentLevel + 1);
            GameManager_paint.gameStatus = GameStatus.Playing;
            swipeControl = new SwipeControl_paint();
            swipeControl.SetLevelManager(this);

            grid = new Grid();
            grid.Initialize(leveldataArray[GameManager_paint.currentLevel].width, leveldataArray[GameManager_paint.currentLevel].height, _cellSize);
            cells = new Cell[leveldataArray[GameManager_paint.currentLevel].width, leveldataArray[GameManager_paint.currentLevel].height];

            CreateGrid(Vector3.zero);

            currentBrush = Instantiate(brushPefab, Vector3.zero, Quaternion.identity);
            currentBrush.currentCoords = leveldataArray[GameManager_paint.currentLevel].brushStartCoords;
            Vector3 brushStartPos = grid.GetCellWorldPosition(leveldataArray[GameManager_paint.currentLevel].brushStartCoords.x, leveldataArray[GameManager_paint.currentLevel].brushStartCoords.y);
            currentBrush.transform.position = brushStartPos;

           gameCamera.ZoomPerspectiveCamera(leveldataArray[GameManager_paint.currentLevel].width, leveldataArray[GameManager_paint.currentLevel].height);
            CompleteBoard();
        }

        private void Update()
        {
            if (swipeControl != null)
            {
                swipeControl.OnUpdate();
            }
        }

        private Cell CreateCells(int x, int y, Vector3 originPos)
        {
            Cell cell = Instantiate(cellPrefab);
            cell.CellCoords = new Vector2Int(x, y);
            cell.GetComponent<Renderer>().material = cubeMat;
            cell.transform.localScale = new Vector3(_cellSize, 0.25f, _cellSize);
            cell.transform.position = originPos + grid.GetCellWorldPosition(x, y);

            return cell;
        }

        public void MoveBrush(Swipe swipe)
        {
            Vector2Int newCoords = grid.GetCellXZBySwipe(currentBrush.currentCoords.x, currentBrush.currentCoords.y, swipe);

            if (newCoords != new Vector2Int(-1, -1))
            {
                SoundManager_paint.Instance.PlayFx(FxType.BrushMove);
                Vector3 finalPos = grid.GetCellWorldPositionBySwipe(currentBrush.currentCoords.x, currentBrush.currentCoords.y, swipe);

                if (!ConnectionAlreadyDone(currentBrush.currentCoords, newCoords, true))
                {
                    inProgressPattern.Add(new ConnectedLine(currentBrush.currentCoords, newCoords));
                    cells[currentBrush.currentCoords.x, currentBrush.currentCoords.y].CellCenterPaint.gameObject.SetActive(true);
                    cells[currentBrush.currentCoords.x, currentBrush.currentCoords.y].CellCenterPaint.material.color = colors[GameManager_paint.currentLevel % colors.Length];
                    LinePaintScript linePaint = Instantiate(linePaintPrefab, new Vector3(0, 0.2f, 0), Quaternion.identity);
                    linePaint.SetRendererPosition(currentBrush.transform.position + new Vector3(0, 0.2f, 0), 
                        finalPos + new Vector3(0, 0.2f, 0), colors[GameManager_paint.currentLevel % colors.Length]);
                    linePaint.SetConnectedCoords(currentBrush.currentCoords, newCoords);
                    connectedLinePaints.Add(linePaint);
                }
                else
                {
                    RemoveConnectLinePaint(currentBrush.currentCoords, newCoords);
                }

                if (leveldataArray[GameManager_paint.currentLevel].completePattern.Count <= inProgressPattern.Count)
                {
                    //Check for win
                    if (IsLevelComplete())
                    {
                        SoundManager_paint.Instance.PlayFx(FxType.Victory);
                        GameManager_paint.gameStatus = GameStatus.Complete;
                        GameManager_paint.currentLevel++;
                        if (GameManager_paint.currentLevel > leveldataArray.Length - 1)
                        {
                            GameManager_paint.currentLevel = 0;
                        }
                        PlayerPrefs.SetInt("CurrentLevel", GameManager_paint.currentLevel);
                        GameManager_paint.totalDiamonds += 15;
                        PlayerPrefs.SetInt("TotalDiamonds", GameManager_paint.totalDiamonds);

                        uIManager.LevelCompleted();
                    }

                }

                currentBrush.transform.position = finalPos;
                currentBrush.currentCoords = newCoords;
            }
        }

        private bool ConnectionAlreadyDone(Vector2Int startCoord, Vector2Int endCoord, bool removeItem)
        {
            bool connected = false;
            for (int i = 0; i < inProgressPattern.Count; i++)
            {
                if (inProgressPattern[i].startCoord == startCoord && inProgressPattern[i].endCoord == endCoord ||
                    inProgressPattern[i].endCoord == startCoord && inProgressPattern[i].startCoord == endCoord)
                {
                    if (removeItem)
                    {
                        inProgressPattern.RemoveAt(i);
                    }

                    connected = true;
                }
            }

            return connected;
        }

        private void RemoveConnectLinePaint(Vector2Int startCoord, Vector2Int endCoord)
        {
            for (int i = 0; i < connectedLinePaints.Count; i++)
            {
                if (connectedLinePaints[i].StartCoord == startCoord && connectedLinePaints[i].EndCoord == endCoord ||
                    connectedLinePaints[i].EndCoord == startCoord && connectedLinePaints[i].StartCoord == endCoord)
                {
                    LinePaintScript line = connectedLinePaints[i];
                    connectedLinePaints.RemoveAt(i);
                    Destroy(line.gameObject);

                    cells[endCoord.x, endCoord.y].CellCenterPaint.gameObject.SetActive(false);
                    return;
                }
            }
        }

        private bool IsLevelComplete()
        {
            //if player has done more connection than required we return false
            if (leveldataArray[GameManager_paint.currentLevel].completePattern.Count != inProgressPattern.Count)
            {
                return false;
            }

            for (int i = 0; i < leveldataArray[GameManager_paint.currentLevel].completePattern.Count; i++)
            {
                if (!ConnectionAlreadyDone(leveldataArray[GameManager_paint.currentLevel].completePattern[i].startCoord, leveldataArray[GameManager_paint.currentLevel].completePattern[i].endCoord, false))
                {
                    return false;
                }
            }

            return true;
        }


        private void CreateGrid(Vector3 originPos)
        {
            for (int x = 0; x < grid.GridArray.GetLength(0); x++)
            {
                for (int y = 0; y < grid.GridArray.GetLength(1); y++)
                {
                    cells[x, y] = CreateCells(x, y, originPos);
                }
            }
        }

        private void CompleteBoard()
        {
            Vector3 offset = new Vector3((leveldataArray[GameManager_paint.currentLevel].width - _cellSize) / 2, 5f, (leveldataArray[GameManager_paint.currentLevel].height - _cellSize) / 2);
            Vector3 gridOriginPos = solutionCamera.transform.position - offset;

            solutionCamera.ZoomOrthographicSizeCamera(leveldataArray[GameManager_paint.currentLevel].width +2, leveldataArray[GameManager_paint.currentLevel].height+2);

            for (int i = 0; i < leveldataArray[GameManager_paint.currentLevel].completePattern.Count; i++)
            {
                Vector3 startPos = gridOriginPos + grid.GetCellWorldPosition(leveldataArray[GameManager_paint.currentLevel].completePattern[i].startCoord);
                Vector3 endPos = gridOriginPos + grid.GetCellWorldPosition(leveldataArray[GameManager_paint.currentLevel].completePattern[i].endCoord);
                LinePaintScript linePaint = Instantiate(linePaintPrefab, new Vector3(0, 0.2f, 0), Quaternion.identity);
                linePaint.SetRendererPosition(startPos + new Vector3(0, 0.2f, 0), 
                    endPos + new Vector3(0, 0.2f, 0), colors[GameManager_paint.currentLevel % colors.Length]);
            }
        }

    }
}