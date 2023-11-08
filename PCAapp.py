import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import sklearn
from sklearn import model_selection
from sklearn.metrics import classification_report
from sklearn.metrics import confusion_matrix
from sklearn.metrics import accuracy_score
from sklearn.linear_model import LogisticRegression
from sklearn.tree import DecisionTreeClassifier
from sklearn.neighbors import KNeighborsClassifier
from sklearn.discriminant_analysis import LinearDiscriminantAnalysis
from sklearn.naive_bayes import GaussianNB
from sklearn.svm import SVC
from sklearn.ensemble import RandomForestClassifier
from sklearn.decomposition import PCA, KernelPCA
from mpl_toolkits.mplot3d import axes3d
import random
import copy
parametersFile = pd.read_csv('C:\\Users\\ramif\\Desktop\\Voip\\PCAInput.csv')

chosenAlgorithm = parametersFile['values'][0]
numOfCalls = parametersFile['values'][1]
numOfSamples = parametersFile['values'][2]
IncludeUnknown = parametersFile['values'][3]

folderName = str(numOfSamples)+'_sec_samples'
activeParams = ['VQ', 'MOS', 'JITTER', 'PLOSS', 'DELAY', 'RERL']

finalData = pd.read_csv("C:\\Users\\ramif\\Desktop\\Voip\\RawData\\"+folderName+"\\"+activeParams[0]+".csv")
finalData.drop('VQ_COLOR_CALLS', axis=1, inplace=True)
for j in list(finalData.columns.values):
      if j != 'EMS_CALL_IDENTIFIER':
            finalData.rename(columns={j: j[len(j)-2:]+"_"+activeParams[0]}, inplace=True)
                
if(len(activeParams) > 1):
    for i in range(len(activeParams)-1):
        tempData = pd.read_csv("C:\\Users\\ramif\\Desktop\\Voip\\RawData\\"+folderName+"\\"+activeParams[i+1]+".csv")
        tempData.drop('VQ_COLOR_CALLS', axis=1, inplace=True)
        for j in list(tempData.columns.values):
            if j != 'EMS_CALL_IDENTIFIER':
                tempData.rename(columns={j: j[len(j)-2:]+"_"+activeParams[i+1]}, inplace=True)
        finalData = pd.merge(finalData,tempData)

finalData = finalData.sample(n = int(numOfCalls), random_state = random.randint(1,101))
N = len(activeParams) * int(numOfSamples)

columns = []
for i in range(int(numOfSamples)):
    VQColorName = 't'+str(i+1)+'_VQ'
    AllFeaturs = finalData[[j for j in list(finalData.columns) if 't'+str(i+1) in j]]
    Response = AllFeaturs[[j for j in list(finalData.columns) if VQColorName in j]]
    AllFeaturs.drop(VQColorName, axis=1, inplace=True)

    #Create graph(2D) with the 2 most valuable pca
    #kpca = KernelPCA(kernel="rbf", fit_inverse_transform=True, gamma=10)
    pca = PCA(n_components=2) #2-dimensional PCA
    
    transformed = pd.DataFrame(pca.fit_transform(AllFeaturs))
    #transformed = pd.DataFrame(kpca.fit_transform(AllFeaturs))
    try:
        Response.reset_index(level=0, inplace=True)
        transformed.reset_index(level=0, inplace=True)
        t=1
        plt.scatter(transformed[Response[VQColorName]==0][0], transformed[Response[VQColorName]==0][1], c='red')
        t=2
        plt.scatter(transformed[Response[VQColorName]==1][0], transformed[Response[VQColorName]==1][1], c='yellow')
        t=3
        plt.scatter(transformed[Response[VQColorName]==2][0], transformed[Response[VQColorName]==2][1], c='green')
        #t=4
        #plt.scatter(transformed[Response[VQColorName]==3][0], transformed[Response[VQColorName]==3][1], c='blue')
        #plt.legend()
        #plt.show()
    except:
        print(t)
        
    FinalPCAData = transformed.copy(deep=True) 
    FinalPCAData["Color"] = Response[VQColorName]
    if(IncludeUnknown=='False'):
        FinalPCAData=FinalPCAData[FinalPCAData.Color != 3] 
        FinalPCAData=FinalPCAData[FinalPCAData.Color != -1]
    fileName = str(i+1) +"_samples_pca_output.csv"
    FinalPCAData.to_csv("C:\\Users\\ramif\\Desktop\\Voip\\RawData\\PCA_results\\"+fileName, index=False)