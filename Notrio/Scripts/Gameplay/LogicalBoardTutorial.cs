using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Takuzu.Generator;
using System.Text;

namespace Takuzu
{
    public class LogicalBoardTutorial : LogicalBoard
    {
        public HashSet<Index2D> interactableIndex;

        protected override void Reveal(Index2D i)
        {
            int value = int.Parse(solution[i.row * puzzleSize + i.column].ToString());
            SetValue(i, value);
            AddImmutableIndex(i);
        }

        public override void RevealRandom()
        {
            List<Index2D> mutableCell = new List<Index2D>();
            for (int i = 0; i < puzzle.Length; ++i)
            {
                for (int j = 0; j < puzzle[0].Length; ++j)
                {
                    Index2D index = new Index2D(i, j);
                    if (!IsImmutableIndex(index))
                        mutableCell.Add(index);
                }
            }

            if (mutableCell.Count > 0)
            {
                List<Index2D> emptyMutableCell = mutableCell.FindAll((index) => { return puzzle[index.row][index.column] == VALUE_EMPTY; });
                Index2D i;
                if (emptyMutableCell.Count > 0)
                {
                    i = emptyMutableCell[UnityEngine.Random.Range(0, emptyMutableCell.Count - 1)];
                }
                else
                {
                    i = mutableCell[UnityEngine.Random.Range(0, mutableCell.Count - 1)];
                }
                StartCoroutine(CrRevealAnim(i));
            }
        }

        public override void Undo()
        {
            if (undoStack == null)
            {
                return;
            }

            if (undoStack.Count == 0)
            {
                return;
            }
            Index2D peek = undoStack.Peek();
            if (GetValue(peek).Equals(VALUE_EMPTY))
            {
                undoStack.RemovePeekAll(peek);
            }

            if (undoStack.Count == 0)
            {
                return;
            }
            peek = undoStack.Peek();
            if (IsImmutableIndex(peek))
            {
                undoStack.RemovePeekAll(peek);
            }

            if (undoStack.Count == 0)
            {
                return;
            }
            peek = undoStack.Peek();
            undoStack.RemovePeekAll(peek);
            SetValue(peek, VALUE_EMPTY);
        }

        protected override void OnMouseClick(Vector2 mousePosition)
        {
            Index2D index = mousePosition.ToIndex2D();
            if (!IsValidIndex(index))
                return;
            if (IsImmutableIndex(index))
                return;
            if (!IsInteractableIndex(index))
                return;

            doActionOnCell(index);
            undoStack.Push(index);

            onCellClicked(index);
        }

        protected bool IsInteractableIndex(Index2D i)
        {
            if (interactableIndex == null)
                return false;
            else
            {
                return interactableIndex.Contains(i);
            }
        }

        public void SetInteractableIndex(params Index2D[] indexes)
        {
            interactableIndex = new HashSet<Index2D>();
            if (indexes == null)
                return;
            for (int i = 0; i < indexes.Length; ++i)
            {
                Index2D index = indexes[i];
                if (index.row != -1 && index.column != -1)
                {
                    interactableIndex.Add(index);
                }
            }
        }

        public void LockMatchedCell()
        {
            for (int i = 0; i < puzzle.Length; ++i)
            {
                for (int j = 0; j < puzzle[0].Length; ++j)
                {
                    Index2D index = new Index2D { row = i, column = j };
                    int cellValue = puzzle[index.row][index.column];
                    int cellSolution = int.Parse(solution[index.row * puzzleSize + index.column].ToString());
                    if (cellValue != VALUE_EMPTY && cellValue == cellSolution)
                    {
                        AddImmutableIndex(index);
                    }
                }
            }
        }
    }
}