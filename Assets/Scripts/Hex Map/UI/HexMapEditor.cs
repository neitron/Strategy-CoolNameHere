﻿using UnityEngine;
using UnityEngine.EventSystems;



public partial class HexMapEditor
{


	public enum OptionalToggle
	{

		Ignore,
		Yes,
		No
			
	}


}



public partial class HexMapEditor : MonoBehaviour
{
	

	[SerializeField] private HexGrid _hexGrid;
	[SerializeField] private Material _terrainMaterial;
	[SerializeField] private Canvas _editorTabCanvas;

	private HexDirection _dragDirection;
	private OptionalToggle _walledMode;
	private OptionalToggle _riverMode;
	private OptionalToggle _roadMode;
	private HexCell _previousCell;
	private HexCell _searchFromCell;
	private HexCell _searchToCell;

	private int _activeTerrainTypeIndex;
	private int _activeSpecialIndex;
	private int _activeWaterLevel;
	private int _activeUrbanLevel;
	private int _activePlantLevel;
	private int _activeFarmLevel;
	private int _activeElevation;
	private int _brushSize;

	private bool _applySpecialIndex = true;
	private bool _applyWaterLevel = true;
	private bool _applyUrbanLevel = true;
	private bool _applyPlantLevel = true;
	private bool _applyFarmLevel = true;
	private bool _applyElevation = true;
	private bool _isDrag;
	private bool _isEditMode;
	


	private void Awake()
	{
		_terrainMaterial.DisableKeyword("GRID_ON");
	}


	private void Update()
	{
		if (Input.GetMouseButton(0) &&
		    !EventSystem.current.IsPointerOverGameObject())
		{
			HandleInput();
		}
		else
		{
			_previousCell = null;
		}
	}


	private void HandleInput()
	{
		if (Camera.main == null)
			return;

		var inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(inputRay, out var hit))
		{
			var currentCell = _hexGrid[hit.point];

			if (_previousCell != null && currentCell != _previousCell)
			{
				ValidDrag(currentCell);
			}
			else
			{
				_isDrag = false;
			}

			if (_isEditMode)
			{
				EditCells(currentCell);
			}
			else if (Input.GetKey(KeyCode.LeftShift) && _searchToCell != currentCell)
			{
				if (_searchFromCell)
				{
					_searchFromCell.DisableHighlight();
				}

				_searchFromCell = currentCell;
				_searchFromCell.EnableHighlight(Color.blue);

				if (_searchToCell)
				{
					_hexGrid.FindPath(_searchFromCell, _searchToCell);
				}
			}
			else if (_searchFromCell && _searchFromCell != currentCell)
			{
				_searchToCell = currentCell;
				_hexGrid.FindPath(_searchFromCell, _searchToCell);
			}

			_previousCell = currentCell;
		}
		else
		{
			_previousCell = null;
		}
	}


	private void ValidDrag(HexCell currentCell)
	{
		for (_dragDirection = HexDirection.Ne; _dragDirection <= HexDirection.Nw; _dragDirection++)
		{
			if (_previousCell[_dragDirection] != currentCell)
				continue;

			_isDrag = true;
			return;
		}
		_isDrag = false;
	}


	private void EditCells(HexCell center)
	{
		var centerX = center.coordinates.x;
		var centerZ = center.coordinates.z;

		for (int r = 0, z = centerZ - _brushSize; z <= centerZ; z++, r++)
		{
			for (var x = centerX - r; x <= centerX + _brushSize; x++)
			{
				EditCell(_hexGrid[new HexCoordinates(x, z)]);
			}
		}

		for (int r = 0, z = centerZ + _brushSize; z > centerZ; z--, r++)
		{
			for (var x = centerX - _brushSize; x <= centerX + r; x++)
			{
				EditCell(_hexGrid[new HexCoordinates(x, z)]);
			}
		}
	}


	private void EditCell(HexCell cell)
	{
		if (cell == null)
			return;

		if (_activeTerrainTypeIndex >= 0)
		{
			cell.terrainTypeIndex = _activeTerrainTypeIndex;
		}

		if (_applyElevation)
		{
			cell.elevation = _activeElevation;
		}

		if (_applyWaterLevel)
		{
			cell.waterLevel = _activeWaterLevel;
		}

		if (_applySpecialIndex)
		{
			cell.specialIndex = _activeSpecialIndex;
		}

		if (_applyUrbanLevel)
		{
			cell.urbanLevel = _activeUrbanLevel;
		}

		if (_applyFarmLevel)
		{
			cell.farmLevel = _activeFarmLevel;
		}

		if (_applyPlantLevel)
		{
			cell.plantLevel = _activePlantLevel;
		}

		if (_riverMode == OptionalToggle.No)
		{
			cell.RemoveRiver();
		}

		if (_roadMode == OptionalToggle.No)
		{
			cell.RemoveRoads();
		}

		if (_walledMode != OptionalToggle.Ignore)
		{
			cell.walled = _walledMode == OptionalToggle.Yes;
		}

		if (!_isDrag)
			return;

		var otherCell = cell[_dragDirection.Opposite()];
		if (otherCell == null)
			return;

		if (_riverMode == OptionalToggle.Yes)
		{
			otherCell.SetOutgoingRiver(_dragDirection);
		}
		if (_roadMode == OptionalToggle.Yes)
		{
			otherCell.AddRoad(_dragDirection);
		}
	}


	public void SetElevation(float elevation)
	{
		_activeElevation = (int)elevation;
	}


	public void SetApplyElevation(bool toggle)
	{
		_applyElevation = toggle;
	}


	public void SetWaterLevel(float level)
	{
		_activeWaterLevel = (int)level;
	}


	public void SetUrbanLevel(float level)
	{
		_activeUrbanLevel = (int)level;
	}


	public void SetFarmLevel(float level)
	{
		_activeFarmLevel = (int)level;
	}


	public void SetPlantLevel(float level)
	{
		_activePlantLevel = (int)level;
	}


	public void SetTerrainTypeIndex(int index)
	{
		_activeTerrainTypeIndex = index;
	}


	public void SetSpecialIndex(float index)
	{
		_activeSpecialIndex = (int)index;
	}


	public void SetApplySpecialIndex(bool toggle)
	{
		_applySpecialIndex = toggle;
	}


	public void SetApplyWaterLevel(bool toggle)
	{
		_applyWaterLevel = toggle;
	}

	public void SetApplyUrbanLevel(bool toggle)
	{
		_applyUrbanLevel = toggle;
	}

	public void SetApplyFarmLevel(bool toggle)
	{
		_applyFarmLevel = toggle;
	}

	public void SetApplyPlantLevel(bool toggle)
	{
		_applyPlantLevel = toggle;
	}


	public void SetBrushSize(float size)
	{
		_brushSize = (int)size;
	}


	public void SetWalledMode(int mode)
	{
		_walledMode = (OptionalToggle)mode;
	}


	public void SetRiverMode(int mode)
	{
		_riverMode = (OptionalToggle)mode;
	}


	public void SetRoadMode(int mode)
	{
		_roadMode = (OptionalToggle)mode;
	}


	public void ShowUi(bool toggle)
	{
		_hexGrid.ShowUi(toggle);
	}


	public void SetEditMode(bool toggle)
	{
		_isEditMode = toggle;
		_editorTabCanvas.enabled = _isEditMode;
		_hexGrid.ShowUi(!toggle);
	}


	public void ShowGrid(bool isVisible)
	{
		if (isVisible)
		{
			_terrainMaterial.EnableKeyword("GRID_ON");
		}
		else
		{
			_terrainMaterial.DisableKeyword("GRID_ON");
		}
	}
}