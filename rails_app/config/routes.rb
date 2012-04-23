PowerStorm::Application.routes.draw do


  get "contact/index"

  get "about" => "about_page#index"
  get "contact" => "contact#index"
 
  match "about" => "about_page#index"
  match "contact" => "contact#index"
  match "admin" => "Admin#index"
  
  resources :buildings
  resources :users
  resources :weathers
  resources :electricity_readings
  resources :meters
  
  root :to => 'Buildings#index'
  
  match 'abr/:abbreviation' => 'Buildings#show'
  
  controller :sessions do
    get 'login' => :new
    post 'login' => :create
    delete 'logout' => :destroy
  end
  
  controller :buildings do
    post 'ajax_update' => :ajax_update
  end
  
  controller :buildings do
	get 'change_view_mode/:view_mode' => :change_view_mode
  end
end
