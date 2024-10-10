﻿using Newtonsoft.Json;
using Solution;
using System.Net.Http.Json;

var token = "9751045c-65f4-4949-8cf3-9361536d9a5512f09d25-7095-465c-8f74-4e9dd7919206";

var matrix = new int[16][];
for (int i = 0; i < 16; i++)
{
    matrix[i] = new int[16];
    for (int j = 0; j < 16; j++)
        matrix[i][j] = -1;
}

var step = 166.66;
var cellsCount = 256;

var client = new HttpClient(new HttpClientHandler());

var watch = System.Diagnostics.Stopwatch.StartNew();

while (cellsCount > 0)
{
    #region analysis
    var sensorDataRaw = client.GetAsync("http://127.0.0.1:8801/api/v1/robot-cells/sensor-data?token=" + token).Result.Content.ReadAsStringAsync().Result;
    var sensorData = JsonConvert.DeserializeObject<SensorData>(sensorDataRaw);

    var direction = Math.Round(sensorData.rotation_yaw);
    if (direction == -180)
        direction *= -1;

    var isWallNorth = false;
    var isWallSouth = false;
    var isWallWest = false;
    var isWallEast = false;

    switch (direction)
    {
        case 0:
            isWallNorth = sensorData.front_distance < step / 2;
            isWallSouth = sensorData.back_distance < step / 2;
            isWallWest = sensorData.left_side_distance < step / 2;
            isWallEast = sensorData.right_side_distance < step / 2;
            break;
        case 90:
            isWallNorth = sensorData.left_side_distance < step / 2;
            isWallSouth = sensorData.right_side_distance < step / 2;
            isWallWest = sensorData.back_distance < step / 2;
            isWallEast = sensorData.front_distance < step / 2;
            break;
        case 180:
            isWallNorth = sensorData.back_distance < step / 2;
            isWallSouth = sensorData.front_distance < step / 2;
            isWallWest = sensorData.right_side_distance < step / 2;
            isWallEast = sensorData.left_side_distance < step / 2;
            break;
        case -90:
            isWallNorth = sensorData.right_side_distance < step / 2;
            isWallSouth = sensorData.left_side_distance < step / 2;
            isWallWest = sensorData.front_distance < step / 2;
            isWallEast = sensorData.back_distance < step / 2;
            break;
    }

    var cellTypeBinary = (isWallNorth ? 1 : 0) * 1 + (isWallEast ? 1 : 0) * 2 + (isWallSouth ? 1 : 0) * 4 + (isWallWest ? 1 : 0) * 8;
    var cellType = 0;

    switch (cellTypeBinary)
    {
        case 1:
            cellType = 2;
            break;
        case 2:
            cellType = 3;
            break;
        case 3:
            cellType = 7;
            break;
        case 4:
            cellType = 4;
            break;
        case 5:
            cellType = 10;
            break;
        case 6:
            cellType = 6;
            break;
        case 7:
            cellType = 11;
            break;
        case 8:
            cellType = 1;
            break;
        case 9:
            cellType = 8;
            break;
        case 10:
            cellType = 9;
            break;
        case 11:
            cellType = 12;
            break;
        case 12:
            cellType = 5;
            break;
        case 13:
            cellType = 13;
            break;
        case 14:
            cellType = 14;
            break;
        case 15:
            cellType = 15;
            break;
    }

    //Bug from api creators X is Y and vice versa
    var cordX = (int)Math.Floor(sensorData.down_y_offset / step + 8);
    var cordY = (int)Math.Floor(sensorData.down_x_offset / step * -1 + 8);

    if (cordX < 0) cordX = 0;
    if (cordY < 0) cordY = 0;

    if (matrix[cordY][cordX] == -1)
    {
        matrix[cordY][cordX] = cellType;
        cellsCount -= 1;
        Console.WriteLine(string.Format($"Cell: x = {cordX}; y = {cordY} has type {cellType}. Cells to go: {cellsCount}"));
    }
    else
    {
        //Build up artificial walls to prevent mouse from looping away from the center of the maze
        switch (direction)
        {
            case 0:
                if (cellType == 1 && matrix[cordY - 1][cordX] > 0 && matrix[cordY][cordX + 1] == -1)
                    isWallNorth = true;
                else if (cellType == 2 && matrix[cordY][cordX - 1] > 0 && matrix[cordY][cordX + 1] == -1)
                    isWallWest = true;
                else if (cellType == 3 && matrix[cordY][cordX - 1] > 0 && matrix[cordY - 1][cordX] == -1)
                    isWallWest = true;
                else if (cellType == 0)
                    if (matrix[cordY][cordX - 1] > 0 && matrix[cordY - 1][cordX] > 0 && matrix[cordY][cordX + 1] == -1)
                    {
                        isWallWest = true;
                        isWallNorth = true;
                    }
                    else if (matrix[cordY][cordX - 1] > 0 && matrix[cordY - 1][cordX] == -1 && matrix[cordY][cordX + 1] > 0)
                    {
                        isWallWest = true;
                    }
                break;
            case 90:
                if (cellType == 2 && matrix[cordY][cordX + 1] > 0 && matrix[cordY + 1][cordX] == -1)
                    isWallEast = true;
                else if (cellType == 3 && matrix[cordY - 1][cordX] > 0 && matrix[cordY + 1][cordX] == -1)
                    isWallNorth = true;
                else if (cellType == 4 && matrix[cordY - 1][cordX] > 0 && matrix[cordY][cordX + 1] == -1)
                    isWallNorth = true;
                else if (cellType == 0)
                    if (matrix[cordY - 1][cordX] > 0 && matrix[cordY][cordX + 1] > 0 && matrix[cordY + 1][cordX] == -1)
                    {
                        isWallNorth = true;
                        isWallEast = true;
                    }
                    else if (matrix[cordY - 1][cordX] > 0 && matrix[cordY][cordX + 1] == -1 && matrix[cordY + 1][cordX] > 0)
                    {
                        isWallNorth = true;
                    }
                break;
            case 180:
                if (cellType == 3 && matrix[cordY + 1][cordX] > 0 && matrix[cordY][cordX - 1] == -1)
                    isWallSouth = true;
                else if (cellType == 4 && matrix[cordY][cordX + 1] > 0 && matrix[cordY][cordX - 1] == -1)
                    isWallEast = true;
                else if (cellType == 1 && matrix[cordY][cordX + 1] > 0 && matrix[cordY + 1][cordX] == -1)
                    isWallEast = true;
                else if (cellType == 0)
                    if (matrix[cordY][cordX + 1] > 0 && matrix[cordY + 1][cordX] > 0 && matrix[cordY][cordX - 1] == -1)
                    {
                        isWallEast = true;
                        isWallSouth = true;
                    }
                    else if (matrix[cordY][cordX + 1] > 0 && matrix[cordY + 1][cordX] == -1 && matrix[cordY][cordX - 1] > 0)
                    {
                        isWallEast = true;
                    }
                break;
            case -90:
                if (cellType == 4 && matrix[cordY][cordX - 1] > 0 && matrix[cordY - 1][cordX] == -1)
                    isWallWest = true;
                else if (cellType == 1 && matrix[cordY + 1][cordX] > 0 && matrix[cordY - 1][cordX] == -1)
                    isWallSouth = true;
                else if (cellType == 2 && matrix[cordY + 1][cordX] > 0 && matrix[cordY][cordX - 1] == -1)
                    isWallSouth = true;
                else if (cellType == 0)
                    if (matrix[cordY + 1][cordX] > 0 && matrix[cordY][cordX - 1] > 0 && matrix[cordY - 1][cordX] == -1)
                    {
                        isWallSouth = true;
                        isWallWest = true;
                    }
                    else if (matrix[cordY + 1][cordX] > 0 && matrix[cordY][cordX - 1] == -1 && matrix[cordY - 1][cordX] > 0)
                    {
                        isWallSouth = true;
                    }
                break;
        }
    }

    #endregion

    #region move
    if (direction == 0 && !isWallWest || direction == 90 && !isWallNorth || direction == 180 && !isWallEast || direction == -90 && !isWallSouth)
    {
        await client.PostAsync("http://127.0.0.1:8801/api/v1/robot-cells/left?token=" + token, null);

        switch (direction)
        {
            case 0:
                direction = -90;
                break;
            case 90:
                direction = 0;
                break;
            case 180:
                direction = 90;
                break;
            case -90:
                direction = 180;
                break;
        }
    }
    else
    {
        while (direction == 0 && isWallNorth || direction == 90 && isWallEast || direction == 180 && isWallSouth || direction == -90 && isWallWest)
        {
            await client.PostAsync("http://127.0.0.1:8801/api/v1/robot-cells/right?token=" + token, null);

            switch (direction)
            {
                case 0:
                    direction = 90;
                    break;
                case 90:
                    direction = 180;
                    break;
                case 180:
                    direction = -90;
                    break;
                case -90:
                    direction = 0;
                    break;
            }
        }
    }

    await client.PostAsync("http://127.0.0.1:8801/api/v1/robot-cells/forward?token=" + token, null);
    #endregion
}

watch.Stop();
var totalSeconds = watch.ElapsedMilliseconds / 1000;
var minutes = totalSeconds / 60;
var seconds = totalSeconds % 60;

var score = client.PostAsync("http://127.0.0.1:8801/api/v1/matrix/send?token=" + token, JsonContent.Create(matrix)).Result.Content.ReadAsStringAsync().Result;
Console.WriteLine(string.Format($"Time spent: {minutes}:{seconds}    {score}"));
