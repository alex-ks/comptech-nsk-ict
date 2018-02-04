import sys
import os

if __name__ == "__main__":
    result = [10]
    
    argFiles  = []
    nameFiles = []
    
    nameFiles = os.listdir(sys.argv[1])
    nameFiles.remove("source.py") 
    for i in range(len(nameFiles)): 
        with open(sys.argv[1]+"\\"+nameFiles[i], 'r') as f:
            arg = f.readline()
        argFiles.append(int(arg)) 
        result[0] = result[0] + 2*argFiles[i]
    result.append(result[0]*2)
    for i in range(len(result)):
        with open(sys.argv[1]+"\\"+str(i)+"output.txt", 'w') as f:
            f.write(str(result[i]))
    print(1)