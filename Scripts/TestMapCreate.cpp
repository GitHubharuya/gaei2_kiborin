#include<bits/stdc++.h>
using namespace std;
using ll = long long;
using ld = long double;
#define rep(i,n) for(ll i=0;i<(n);i++)
#define drep(i,n) for(ll i=(n)-1;i>=0;i--)
#define rrep(i,n) for(ll i=1;i<(n);i++)
vector<int> dx = {1,0,-1,0};
vector<int> dy = {0,1,0,-1};

int main() {
  int n,seed;
  cout << "Nは奇数のみ : ";
  cin >> n;
  cout << "シード値 : ";
  cin >> seed;
  srand(seed);
  vector<string> s(n,string(n,'#'));
  vector<vector<bool>> v(n,vector<bool>(n));
  int nowx = (rand()%((n-1)/2))*2+1, nowy = (rand()%((n-1)/2))*2+1;
  deque<pair<int,int>> q;
  q.emplace_back(nowx,nowy);
  v[nowx][nowy] = true;
  while(!q.empty()){
    auto [x,y] = q.back(); q.pop_back();
    int next = rand()%4;
    rep(i,4){
      int idx = (next+i)%4;
      int ni = x+dx[idx]*2, nj = y+dy[idx]*2;
      if(ni<0 || ni>=n || nj<0 || nj>=n) continue;
      if(v[ni][nj]) continue;
      v[ni][nj] = true;
      v[ni-dx[idx]][nj-dy[idx]] = true;
      q.emplace_back(ni,nj);
    }
  }
  rrep(i,n-1){
    rrep(j,n-1){
      if(!v[i][j])if(rand()%2) v[i][j] = true;
    }
  }
  rep(i,n){
    rep(j,n){
      if(v[i][j]) cout << '.';
      else cout << '#';
    }
    cout << '\n';
  }
}