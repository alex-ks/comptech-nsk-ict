import sys
import os




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
    
    nameFiles = os.listdir(sys.argv[1]) #список файлов
    nameFiles.remove("source.py") #удаление из списка
    for i in range(len(nameFiles)):
        with open(os.path.join(sys.argv[1],nameFiles[i]), 'r') as f:
            arg = f.readline()
        argFiles.append((arg)) #строки входов 
    if (len(argFiles) == 2):
        # разбиение строк
        for i in range(len(argFiles)):
            if isInt(argFiles[i]):
                values.append(int(argFiles[i]))
            else:
                values.append(float(argFiles[i]))
                
        #Операция **
        result = values[0] ** values[1]
        
        with open(os.path.join(sys.argv[1],str(i)+"output.txt"), 'w') as f:
            f.write(str(result))
        print(1)
    else:
        print(0)
