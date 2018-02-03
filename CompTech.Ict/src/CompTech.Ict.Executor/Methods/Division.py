import sys
import os




def isInt(s):
    try:
        int(s)
        return True
    except ValueError:
        return False

if __name__ == "__main__":
    
    
    argFiles  = []
    nameFiles = []
    values = [] 
    result = 0
    
    nameFiles = os.listdir(sys.argv[1]) 
    nameFiles.remove("source.py") 
    for i in range(len(nameFiles)):
        with open(os.path.join(sys.argv[1],nameFiles[i]), 'r') as f:
            arg = f.readline()
        argFiles.append((arg)) 
    flagIntegers = True        
    if (len(argFiles) == 2):
        
        for i in range(len(argFiles)):

            if isInt(argFiles[i]):
                values.append(int(argFiles[i]))
            else:
                values.append(float(argFiles[i]))
                flagIntegers = False
                
        
        if flagIntegers:
            result = (int)(values[0] / values[1])
        else:
            result = values[0] / values[1]
        
        with open(os.path.join(sys.argv[1],str(i)+"output.txt"), 'w') as f:
            f.write(str(result))
        print(1)
    else:
        print(0)
