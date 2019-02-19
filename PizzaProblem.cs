using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace pizzaSolution
{
    public class PizzaProblem
    {
        // string dataFile = "a_example.in";
        // string dataFile = "b_small.in";
        // string dataFile = "c_medium.in";
        string dataFile = "d_big.in";


        // string resultFile = "a_result.txt";
        // string resultFile = "b_result.txt";
        // string resultFile = "c_result.txt";
        string resultFile = "d_result.txt";
        int rowCount;
        int columnCount;
        int minimumNumberOfEachIngredient;
        int maxNumberOfCells;
        int mushroomCount = 0;
        int tomatoeCount = 0;

        public async Task Main()
        {
            var foundSlices = new List<PizzaSlice>();

            var data = await ReadDataAsync();
            var ingredientToSearch = DetermineWhichIngredientIsLeast(mushroomCount, tomatoeCount);
            var sliceUsageData = CreateMatrixOfUnusedCells(data);
            var emptyCycles = 0;

            while (true)
            {
                var pointToUse = GetUnusedStartingPoint(data, sliceUsageData, ingredientToSearch);
                if (pointToUse.Y == -1)
                    break; // we didn't find the next Point

                var shapes = GeneratePossibleShapes(maxNumberOfCells, minimumNumberOfEachIngredient);

                var fittingShape = FindFirstFittingShape(pointToUse, data, sliceUsageData, shapes);
                if (fittingShape is null) {
                    sliceUsageData[pointToUse.X][pointToUse.Y] = true;
                    emptyCycles++;
                    continue; // no possible shapes here
                }

                if (emptyCycles > 10000) {
                    break;
                }

                foundSlices.Add(new PizzaSlice
                {
                    X1 = pointToUse.X - fittingShape.Height + 1,
                    Y1 = pointToUse.Y - fittingShape.Width + 1,
                    X2 = pointToUse.X,
                    Y2 = pointToUse.Y
                });

                MarkSliceAsTaken(pointToUse, sliceUsageData, fittingShape);
            }

            await PrintResult(foundSlices);
        }

        private async Task PrintResult(List<PizzaSlice> slices)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), resultFile);
            using (var fileStream = new FileStream(path, FileMode.Create))
            using (var streamWriter = new StreamWriter(fileStream, Encoding.ASCII, 1024, true))
            {
                await streamWriter.WriteLineAsync (slices.Count.ToString ());

                for (int i = 0; i < slices.Count; i++) {
                    var slice = slices[i];
                    await streamWriter.WriteLineAsync ($"{slice.X1} {slice.Y1} {slice.X2} {slice.Y2}");
                }
            }
        }

        private void MarkSliceAsTaken(Point pointToUse, List<List<bool>> sliceUsageData, Shape fittingShape)
        {
            for (int i = pointToUse.X; i >= pointToUse.X - fittingShape.Height + 1; i--)
            {
                for (int j = pointToUse.Y; j >= pointToUse.Y - fittingShape.Width + 1; j--)
                {
                    sliceUsageData[i][j] = true;
                }
            }
        }

        private Shape FindFirstFittingShape(Point startingPoint,
                                            List<List<PizzaIngredient>> data,
                                            List<List<bool>> sliceUsageData,
                                            List<Shape> shapes)
        {
            for (var i = 0; i < shapes.Count; i++)
            {
                var shape = shapes[i];
                if (startingPoint.Y - shape.Width + 1 >= 0 &&
                    startingPoint.X - shape.Height + 1 >= 0)
                {
                    var hasIngredients = SpaceHasIngredientCountNecessary(startingPoint, data, shape, minimumNumberOfEachIngredient);
                    if (!hasIngredients)
                        continue;

                    var cellsAreFree = CheckIfCellsAreFree(startingPoint, shape, sliceUsageData);
                    if (!cellsAreFree)
                        continue;

                    return shape;
                }
            }

            return null;
        }

        private bool CheckIfCellsAreFree(Point startingPoint, Shape shape, List<List<bool>> sliceUsageData)
        {
            for (int i = startingPoint.X; i >= startingPoint.X - shape.Height + 1; i--)
            {
                for (int j = startingPoint.Y; j >= startingPoint.Y - shape.Width + 1; j--)
                {
                    if (sliceUsageData[i][j] == true)
                    {
                        return false; // slice is already taken
                    }
                }
            }

            return true;
        }

        private bool SpaceHasIngredientCountNecessary(
            Point startPoint,
            List<List<PizzaIngredient>> data,
            Shape shape,
            int minimumNumberOfEachIngredient)
        {
            int mushroomCOunt = 0;
            int tomatoeCount = 0;
            for (int i = startPoint.X; i >= startPoint.X - shape.Height + 1; i--)
            {
                for (int j = startPoint.Y; j >= startPoint.Y - shape.Width + 1; j--)
                {
                    if (data[i][j].Ingredient == Ingredient.Mushroom)
                    {
                        mushroomCOunt++;
                    }
                    else
                    {
                        tomatoeCount++;
                    }
                }
            }

            if (mushroomCOunt >= minimumNumberOfEachIngredient &&
                tomatoeCount >= minimumNumberOfEachIngredient)
            {
                return true;
            }

            return false;
        }

        private List<Shape> GeneratePossibleShapes(int maxNumberOfCells, int minimumNumberOfEachIngredient)
        {
            var shapes = new List<Shape>();
            var minimumCellCount = minimumNumberOfEachIngredient * 2;

            for (var i = 1; i <= maxNumberOfCells; i++)
            {
                for (var j = 1; j <= maxNumberOfCells; j++)
                {
                    var plotas = i * j;
                    if (plotas > maxNumberOfCells)
                        break;
                    if (plotas < minimumCellCount)
                        continue;

                    shapes.Add(new Shape(i, j));
                }
            }

            shapes.Sort (new ShapeComparer ());
            return shapes;
        }

        private Point GetUnusedStartingPoint(List<List<PizzaIngredient>> data, List<List<bool>> usageData, Ingredient ingredientToSearchFor)
        {
            var rnd = new Random ();
            var iIndex = rnd.Next (0, data.Count - 1);
            var jIndex = rnd.Next (0, data[0].Count - 1);

            for (var i = iIndex; i < data.Count; i++)
            {
                for (var j = jIndex; j < data[0].Count; j++)
                {
                    if (data[i][j].Ingredient == ingredientToSearchFor &&
                        usageData[i][j] == false)
                    {
                        return new Point(i, j);
                    }
                }
            }

            for (var i = iIndex - 1; i >= 0; i--)
            {
                for (var j = jIndex - 1; j >= 0; j--)
                {
                    if (data[i][j].Ingredient == ingredientToSearchFor &&
                        usageData[i][j] == false)
                    {
                        return new Point(i, j);
                    }
                }
            }

            return new Point(-1, -1);
        }

        private List<List<bool>> CreateMatrixOfUnusedCells(List<List<PizzaIngredient>> data)
        {
            var usageList = new List<List<bool>>();
            data.ForEach((row) =>
            {
                var usageRow = new List<bool>();
                row.ForEach(ingredient =>
                {
                    usageRow.Add(false);
                });
                usageList.Add(usageRow);
            });

            return usageList;
        }

        private Ingredient DetermineWhichIngredientIsLeast(int mushrooms, int tomatoes)
        {
            if (mushrooms < tomatoes)
                return Ingredient.Mushroom;
            return Ingredient.Tomatoe;
        }

        private async Task<List<List<PizzaIngredient>>> ReadDataAsync()
        {
            var data = new List<List<PizzaIngredient>>();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "data", dataFile);

            // R C L H
            using (var fileStream = new FileStream(path, FileMode.Open))
            using (var streamReader = new StreamReader(fileStream))
            {
                var firstLine = await streamReader.ReadLineAsync();
                var chars = firstLine.Split(' ');
                int.TryParse(chars[0], out rowCount);
                int.TryParse(chars[1], out columnCount);
                int.TryParse(chars[2], out minimumNumberOfEachIngredient);
                int.TryParse(chars[3], out maxNumberOfCells);

                while (streamReader.Peek() != -1)
                {
                    var dataLine = await streamReader.ReadLineAsync();
                    var ingredientLine = new List<PizzaIngredient>();
                    for (var i = 0; i < dataLine.Length; i++)
                    {
                        if (dataLine[i] == 'M')
                        {
                            ingredientLine.Add(new PizzaIngredient(Ingredient.Mushroom));
                            mushroomCount++;
                        }

                        else if (dataLine[i] == 'T')
                        {
                            ingredientLine.Add(new PizzaIngredient(Ingredient.Tomatoe));
                            tomatoeCount++;
                        }
                    }

                    data.Add(ingredientLine);
                }
            }

            return data;
        }
    }
}