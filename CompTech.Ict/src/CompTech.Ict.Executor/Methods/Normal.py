import sys
import os
import scipy.stats as stats

def isInt(s):
    try:
        int(s)
        return True
    except ValueError:
        return False
    
# на вход -- папка со входами
if __name__ == "__main__":
    
    
    argFiles  = [] # файлы
    nameFiles = [] # строки файлов
    values = [] # числовые значения
    result = 0
    results = []
    nameFiles = os.listdir(sys.argv[1]) #список файлов
    nameFiles.remove("source.py") #удаление из списка
    with open(os.path.join(sys.argv[1],nameFiles[0]), 'r') as f:
        arg = f.readline()
    # разбиение строк
    values = arg.split(' ')
    if (len(values) >= 20):
        for i in range(len(values)):
            results.append(float(values[i]))
        #Операция проверки нормального распределения 
        statist,hi_2 = stats.normaltest(results)
        with open(os.path.join(sys.argv[1],str(1)+"output.txt"), 'w') as f:
            f.write(str(hi_2))
        print(1)
    else:
        print(0)
    
