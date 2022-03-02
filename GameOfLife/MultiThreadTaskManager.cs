//#define USING_SEMAPHORE_SOLUTION
//#define USING_DIVISION_SOLUTION
#define USING_DINAMIC_SOLUTION
using System;
using System.Collections.Generic;
using System.Threading;

namespace GameOfLife
{
    class MultiThreadTaskManager
    {
        public const int MaxNumberOfThreads = 8;
        public static bool exit = false;
        //Only for testing
        private volatile static int numberOfThreads = 0;

        private volatile static int doneThreads = 0;
        //Matrix for read
        private static bool[,] originalMatrix;
        //Matrix for write
        private static bool[,] copyMatrix;

        private static int rows;
        private static int columns;
        private static object lockObj = new object();
        private static Semaphore waitingThread = new Semaphore(MaxNumberOfThreads, MaxNumberOfThreads);

#if USING_DIVISION_SOLUTION
        private static int numOfCellsPerThread;
#endif
#if USING_DINAMIC_SOLUTION
        private static Thread[] threads = new Thread[MaxNumberOfThreads];
        private static int[] taskArrived = new int[MaxNumberOfThreads];
#endif
#if USING_SEMAPHORE_SOLUTION
        public static void addTask(int index)
        {
        //Thanks to the semaphore there can only been a max number of threads at the same time
            waitingThread.WaitOne();
            {
                Thread thread = new Thread(cellThread);
                thread.IsBackground = true;
                thread.Start(index);
            }
        }

        public static void cellThread(object indexObj)
        {

            //Get the x an y 
            int y = (int)indexObj % columns;
            int x = ((int)indexObj -y)/columns;

            //For debugging
            int threadNumber = numberOfThreads;
            ++numberOfThreads;
            string threadName = string.Format("Thread {0}", threadNumber);
            Thread.CurrentThread.Name = threadName;
            //Console.WriteLine("Thread {0} - Cell [{1},{2}]", threadNumber, x, y);

            //Cell functions
            int neighbours = getNeighbours(x, y);
            rulesOfLife(x, y, neighbours);

            //Only one thread per time will increase the variable
            lock(lockObj)
            {
                doneThreads++;
            }
            //Debugging
            //Thread.Sleep(100);

            //When a thread is finished we release the semaphore for let another one to start
            waitingThread.Release();

        }
#endif
#if USING_DIVISION_SOLUTION
        public static void addTask()
        {
        //Only create the max number of threads allowed
            for (int i = 0; i < MaxNumberOfThreads; i++)
            {
                Thread thread = new Thread(cellThread);
                thread.IsBackground = true;
                //Pass the starting cell of each thread
                thread.Start(numOfCellsPerThread * i);
            }
        }
        public static void cellThread(object indexObj)
        {
            //Get the x an y 
            int y = (int)indexObj % columns;
            int x = ((int)indexObj - y) / columns;

            //For debugging
            int threadNumber = numberOfThreads;
            ++numberOfThreads;
            string threadName = string.Format("Thread {0}", threadNumber);
            Thread.CurrentThread.Name = threadName;

            //Debugging
            //Console.WriteLine("Thread {0} - Cell [{1},{2}]", threadNumber, x, y);

            int cells;
            //Check if the number of cells per thread is going to be the predefinide (rows*col)/numMaxThreads or if its going to be less for not get out of index
            if((int)indexObj+numOfCellsPerThread >rows*columns)
            {
                cells = (int)indexObj - rows * columns - 1;
            }
            else
            {
                cells = numOfCellsPerThread;
            }
            //Cell functions
            for (int i = 0; i < cells; i++)
            {
                y = (int)indexObj % columns;
                x = ((int)indexObj - y) / columns;
                int neighbours = getNeighbours(x, y);
                rulesOfLife(x, y, neighbours);
                indexObj = (int)indexObj + 1;
            }
            //Only one thread per time will increase the variable
            lock (lockObj)
            {
                doneThreads++;
            }
            
        }
#endif
#if USING_DINAMIC_SOLUTION
        public static void addTask(object index)
        {
            //Each cell is trying to add a task to the manager, but it only happens if thers an available thread
            waitingThread.WaitOne();
            //Instead of using semaphores we can use a while loop until you can make the task, but we will use a semaphore for show the use of it
            //while(!stop)
            {
                for (int i = 0; i < MaxNumberOfThreads; i++)
                {
                    //If we have a waiting thread we will assign it a task changing the cell index
                   if (taskArrived[i]==-1)
                    {
                        taskArrived[i] = (int)index;
                        break;
                    }
                }
            }
           
        }
        /*
         * The thread will be allways executed, but unlease it has an valid cell it wont do nothing 
         */
        public static void cellThread()
        {
            while (!exit)
            {
                if (taskArrived[int.Parse(Thread.CurrentThread.Name)]!=-1)
                {
                    int y = (int)taskArrived[int.Parse(Thread.CurrentThread.Name)] % columns;
                    int x = ((int)taskArrived[int.Parse(Thread.CurrentThread.Name)] - y) / columns;
                    //Debugging
                    //Console.WriteLine("{0} - Cell [{1},{2}]", Thread.CurrentThread.Name, x, y);

                    //Cell functions
                    int neighbours = getNeighbours(x, y);
                    rulesOfLife(x, y, neighbours);

                    //Only one thread per time will increase the variable
                     lock (lockObj)
                     {
                         doneThreads++;
                    }
                     // change thread state to let him now that he done the work with this cell
                    taskArrived[int.Parse(Thread.CurrentThread.Name)] = -1;
                    waitingThread.Release();
                    //Thread.Sleep(5000);
                }
                else
                {
                    Thread.Sleep(0);
                }
            }
        }
#endif
        private static int getNeighbours(int row, int column)
        {
            int aliveNeighbors = 0;
            // find your alive neighbors
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (row + i >= 0 && row + i < rows && column + j >= 0 && column + j < columns)
                        if (originalMatrix[row + i, column + j])
                        {
                            aliveNeighbors++;
                        }
                }
            }
            // The cell needs to be subtracted from its neighbors as it was  counted before 
            if (originalMatrix[row, column])
            {
                aliveNeighbors--;
            }

            return aliveNeighbors;
        }
        private static void rulesOfLife(int row, int column, int aliveNeighbors)
        {
            // Implementing the Rules of Life 
            bool currentCell = originalMatrix[row, column];
            // Cell is lonely and dies 
            if (currentCell == true && aliveNeighbors < 2)
            {
                copyMatrix[row, column] = false;
            }

            // Cell dies due to over population 
            else if (currentCell == true && aliveNeighbors > 3)
            {
                copyMatrix[row, column] = false;
            }

            // A new cell is born 
            else if (currentCell == false && aliveNeighbors == 3)
            {
                copyMatrix[row, column] = true;
            }
            // stays the same
            else
            {
                copyMatrix[row, column] = currentCell;
            }
        }

        /*
         * Initialize variables
         */
        public static void setVariables(int r, int c)
        {
            rows = r;
            columns = c;
#if USING_DIVISION_SOLUTION
            //Calculate the number of cells that each thread must procese
            numOfCellsPerThread = rows * columns / MaxNumberOfThreads;
            if(numOfCellsPerThread % 1 !=0)
            {
                numOfCellsPerThread++;
            }
#endif
#if USING_DINAMIC_SOLUTION
            //Create the threads
            for (int i = 0; i < MaxNumberOfThreads; i++)
            {
                taskArrived[i] = -1;
                threads[i] = new Thread(cellThread);
                threads[i].Name = i.ToString();
                threads[i].IsBackground = true;
                threads[i].Start();
            }
#endif
        }
        /*
         Reset the variables of the new iteration and add the old matrix to modify 
         */
        public static void setMatrix(bool[,] matrix)
        {
            numberOfThreads = 0;
            doneThreads = 0;
            originalMatrix = matrix;
            copyMatrix = new bool[rows,columns];
        }
        public static bool[,] getMatrix()
        {
            return copyMatrix;
        }
        public static int getNumOfFinishedThreads()
        {
            return doneThreads;
        }
        public static void exitTask()
        {
            exit = true;
        }
    }
}

